using System.Reflection;
using MoonSharp.Interpreter;

namespace RoHelper;


class LuaScript
{
    private const int DefaultAutoYieldCounter = 1000;
    private static string? luaCodeForGlobals;
    private static string getGlobalLuaCode(){
        if (luaCodeForGlobals==null){
            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream? stream = assembly.GetManifestResourceStream("RoHelper.resources.globals.lua"))
            {
                if (stream==null){
                    Console.WriteLine("globals.lua is null");
                    luaCodeForGlobals = "";
                }else luaCodeForGlobals = (new StreamReader(stream)).ReadToEnd().Replace("\n"," ");
            }
        } return luaCodeForGlobals;
    }
    private DynValue? MainFunction;
    public string scriptName = "";
    public Script luaScript = new Script();
    private bool _isCompiled = false;
    public bool isCompiled{get=>_isCompiled;}
    public SyntaxErrorException? SyntaxError;
    public void Compile(string luaCode){
        var Code = "return function()"+getGlobalLuaCode()+" "+luaCode+"\nend";
        try{
            MainFunction = luaScript.DoString(Code);
            _isCompiled = true;
        }catch(SyntaxErrorException exc){
            Console.WriteLine("LuaScript syntax error: "+exc.DecoratedMessage);
            SyntaxError = exc;
        }
    }
    private bool isScrRunning = false;
    public bool isRunning{
        get=>isScrRunning;
        set{ isScrRunning=value;
            if (value) RunScript();
            else StopScript();
        }
    }
    private readonly object _lock = new object();
    private HashSet<Coroutine> Coroutines = new();
    public void startCoroutine(Coroutine coroutine, DynValue[]? args =null){
        lock (_lock){
            Coroutines.Add(coroutine);
            if (args!=null)CoroutinesArgs[coroutine] = args;
        } _newCoroutineStarted.Set();
    }
    private ManualResetEventSlim _runningEvent = new(false);
    public ManualResetEventSlim _eventPausedUnlock = new(false);
    public ManualResetEventSlim _newCoroutineStarted = new(false);
    private Coroutine? RunningCoroutine;
    private Dictionary<Coroutine, DateTime> _sleepingCoroutines = new();
    public Dictionary<Coroutine, DynValue[]> CoroutinesArgs = new();
    private HashSet<Coroutine> _PausedCoroutines = new();
    public LuaScript(){
        MakeGlobalENV();
        Task.Run(()=>{
            while (true){
                _runningEvent.Wait();
                if (isScrRunning){
                    DateTime? nearWakeUp = null;
                    Coroutine[] snapshot;
                    lock (_lock) snapshot = Coroutines.ToArray();
                    foreach (Coroutine coroutine in snapshot){
                        if (isScrRunning&&Coroutines.Contains(coroutine)){
                            lock (_lock){
                                if (_PausedCoroutines.Contains(coroutine)) continue;
                                if (_sleepingCoroutines.TryGetValue(coroutine, out var resumeTime)){
                                    if (DateTime.UtcNow < resumeTime) {
                                        if (nearWakeUp==null||nearWakeUp>resumeTime) nearWakeUp = resumeTime;
                                        continue;
                                    } _sleepingCoroutines.Remove(coroutine);
                                }
                            } coroutine.AutoYieldCounter = DefaultAutoYieldCounter;
                            RunningCoroutine = coroutine;
                            try{
                                DynValue[]? args = null;
                                lock(_lock) if (CoroutinesArgs.TryGetValue(coroutine,out var arg))
                                    args = arg;
                                DynValue result; 
                                if (args!=null){
                                    result = coroutine.Resume(args);
                                    lock(_lock)CoroutinesArgs.Remove(coroutine);
                                }else result = coroutine.Resume();
                                // coroutine exit handler
                                if (result.Type != DataType.YieldRequest){
                                    if (result.Type!=DataType.UserData||result.ToObject()!=scriptMethods){
                                        lock(_lock) Coroutines.Remove(coroutine);
                                    }
                                }
                            }catch(ScriptRuntimeException exc){
                                Console.WriteLine("LuaScript error: "+exc.DecoratedMessage);
                                lock(_lock) Coroutines.Remove(coroutine);
                            }
                        }
                    } RunningCoroutine = null;
                    int waitingCount;int pausedCount; int Count;
                    lock (_lock){
                        pausedCount = _PausedCoroutines.Count;
                        waitingCount = pausedCount+_sleepingCoroutines.Count;
                        Count = Coroutines.Count;
                    } if (Count==waitingCount){
                        _newCoroutineStarted.Reset();
                        _eventPausedUnlock.Reset();
                        if (nearWakeUp!=null){
                            TimeSpan time = (TimeSpan)(nearWakeUp-DateTime.UtcNow);
                            if (time>TimeSpan.FromMilliseconds(0)){
                                _eventPausedUnlock.Wait(time);
                            }else Thread.Sleep(0);
                        }else if (pausedCount==waitingCount) _eventPausedUnlock.Wait();
                        else if (Count==0) _newCoroutineStarted.Wait();
                    }
                }
            }
        });
    }
    public void skipThisCoroutine(){
        if (RunningCoroutine!=null){
            RunningCoroutine.AutoYieldCounter = 1;
        }
    }
    public void RunScript(){
        if (isScrRunning||MainFunction==null) return;
        isScrRunning = true;
        DynValue coroutine = luaScript.CreateCoroutine(MainFunction);
        lock (_lock) Coroutines.Add(coroutine.Coroutine);
        _runningEvent.Set();
    }
    public event EventHandler? scriptStopped;
    public void StopScript(){
        if (!isScrRunning) return;
        isScrRunning = false;
        _runningEvent.Reset();
        lock (_lock){
            Coroutines = new();
            _sleepingCoroutines = new();
            _PausedCoroutines = new();
            CoroutinesArgs = new();
        } skipThisCoroutine();
        scriptStopped?.Invoke(this,EventArgs.Empty);
    }
    public ScriptMethods scriptMethods = new();
    private void MakeGlobalENV(){
        luaScript.Globals["print"] = (Action<object?>)((o)=>{
            var str = (o ?? "nil").ToString();
            Console.WriteLine(scriptName+": "+str);
        });
        luaScript.Globals["wait"] = (Action<double>)((s)=>{
            var coroutine = RunningCoroutine;
            if (coroutine==null){
                Thread.Sleep(TimeSpan.FromSeconds(s));
                return;
            } coroutine.AutoYieldCounter = 1;
            if (s > 0){
                var time = DateTime.UtcNow.AddSeconds(s);
                lock(_lock)_sleepingCoroutines[coroutine] = time;
            }
        });
        luaScript.Globals["spawn"] = (Action<DynValue>)((func) =>{
            DynValue coroutine = luaScript.CreateCoroutine(func);
            lock (_lock) Coroutines.Add(coroutine.Coroutine);
            RunningCoroutine = coroutine.Coroutine;
            coroutine.Coroutine.AutoYieldCounter = DefaultAutoYieldCounter;
            coroutine.Coroutine.Resume();
        });
        luaScript.Globals["getClassTypeName"] = (Func<object,string>)((obj) =>{
            if (obj is LuaEvent) return "ScriptEvent";
            if (obj==scriptMethods) return "Script";
            return "unknown";
        });
        luaScript.Globals["script"] = scriptMethods;
        luaScript.Globals["classGetMembersWriteTypes"] = (Func<object?,Table?>)((obj)=>{
            if (obj==null) return null;
            var result = new Table(luaScript); //<string,bool|string>
            var members = obj.GetType().GetMembers();
            foreach (var member in members){
                result[member.Name] = true;
                if (member.MemberType==MemberTypes.Property){
                    PropertyInfo prop = (PropertyInfo)member;
                    if (prop.CanWrite) result[member.Name] = GetLuaTypeName(prop.PropertyType);
                }
            }
            return result;
        });
    }
    [MoonSharpUserData]
    public class LuaEvent{
        private LuaScript script;
        private HashSet<DynValue> ConnectedFuncs = new();
        private HashSet<Coroutine> OnceCoroutines = new();
        private HashSet<Coroutine> PausedCoroutines = new();
        [MoonSharpHidden]
        public event EventHandler? theEvent;
        public LuaEvent(LuaScript script){
            this.script = script;
            //EventHandler
            script.scriptStopped += (_,_)=>{
                ConnectedFuncs = new();
                OnceCoroutines = new();
                PausedCoroutines = new();
            };
        } 
        public DynValue[]? _lastArgs = null;
        [MoonSharpHidden]
        public void trigger(DynValue[]? args =null){
            theEvent?.Invoke(this,EventArgs.Empty);
            lock(script._lock){
                _lastArgs = args;
                foreach (var func in ConnectedFuncs){
                    var Coroutine = script.luaScript.CreateCoroutine(func);
                    script.Coroutines.Add(Coroutine.Coroutine);
                    if (args!=null) script.CoroutinesArgs[Coroutine.Coroutine] = args;
                } foreach (var Coroutine in OnceCoroutines){
                    script.Coroutines.Add(Coroutine);
                    if (args!=null) script.CoroutinesArgs[Coroutine] = args;
                }
            } foreach (var Coroutine in PausedCoroutines){
                lock(script._lock)script._PausedCoroutines.Remove(Coroutine);
            } if (PausedCoroutines.Count!=0){
                PausedCoroutines = new();
                script._eventPausedUnlock.Set();
            } if (ConnectedFuncs.Count!=0||OnceCoroutines.Count!=0){
                script._eventPausedUnlock.Set();
                script._newCoroutineStarted.Set();
                OnceCoroutines = new();
            }
        }
        public void Connect(DynValue func){
            ConnectedFuncs.Add(func);
        }
        public void Once(DynValue func){
            DynValue coroutine = script.luaScript.CreateCoroutine(func);
            OnceCoroutines.Add(coroutine.Coroutine);
        }
        public void Wait(){
            var Running = script.RunningCoroutine;
            if (Running!=null){
                PausedCoroutines.Add(Running);
                script.skipThisCoroutine();
                lock(script._lock)script._PausedCoroutines.Add(Running);
            }
        }
    }
    // работай чат джепете
    public static string GetLuaTypeName(Type t)
    {
        // number
        if (t == typeof(sbyte) || t == typeof(byte) || t == typeof(short) || t == typeof(ushort) ||
            t == typeof(int) || t == typeof(uint) || t == typeof(long) || t == typeof(ulong) ||
            t == typeof(float) || t == typeof(double) || t == typeof(decimal))
                return "number";
        // string
        if (t == typeof(string) || t == typeof(char) || t == typeof(System.Text.StringBuilder))
            return "string";
        // boolean
        if (t == typeof(bool))
            return "boolean";
        // table (упрощённо, без проверки всех интерфейсов)
        if (t == typeof(MoonSharp.Interpreter.Table) || 
            typeof(System.Collections.IList).IsAssignableFrom(t) ||
            typeof(System.Collections.IDictionary).IsAssignableFrom(t) ||
            typeof(System.Collections.IEnumerable).IsAssignableFrom(t))
                return "table";
        return "userdata";
    }
}