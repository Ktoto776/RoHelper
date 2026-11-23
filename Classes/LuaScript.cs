using System.Reflection;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

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
                }else{
                    luaCodeForGlobals = "do "+(new StreamReader(stream)).ReadToEnd()+"\nend";
                }
            }
        } return luaCodeForGlobals;
    }
    private DynValue? MainFunction;
    public string scriptName = "";
    public Script luaScript;
    private bool _isCompiled = false;
    public bool isCompiled{get=>_isCompiled;}
    public SyntaxErrorException? SyntaxError;
    private string? lastError;
    public void InitGlobals(){
        luaScript.Globals["getClassTypeName"] = (Func<object,string>)((obj)=>
            {return GetLuaClassName(obj.GetType());});
        luaScript.Globals["classGetMembersWriteTypes"] = (Func<object?,Table?>)((obj)=>{
            if (obj==null) return null;
            var result = new Table(luaScript); //<string,bool|string>
            var members = obj.GetType().GetMembers();
            foreach (var member in members){
                result[member.Name] = true;
                if (member.MemberType==MemberTypes.Property){
                    PropertyInfo prop = (PropertyInfo)member;
                    if (prop.CanWrite) result[member.Name] = GetLuaType(prop.PropertyType);
                }
            }
            return result;
        });
        luaScript.Globals["_changeEnvironment"] = (Action<string,DynValue>)((name,value)=>{
            luaScript.Globals[name] = value;
        });
        luaScript.Globals["_setLastError"] = (Action<string>)((err)=>{
            lastError = err;
        });
        try{
            luaScript.DoString(getGlobalLuaCode(),null,"_GLOBALS");
        }catch(ScriptRuntimeException e){
            Console.WriteLine("Error when init globals: "+e.DecoratedMessage);
        }catch(SyntaxErrorException e){
            Console.WriteLine("Syntax error when init globals: "+e.DecoratedMessage);
        }
        luaScript.Globals["_changeEnvironment"] = DynValue.Nil;
        luaScript.Globals["_setLastError"] = DynValue.Nil;
        luaScript.Globals["getClassTypeName"] = DynValue.Nil;// Lua alternative: typeof()
        globalsInited = true;
    }
    public bool globalsInited = false;
    public void Compile(string luaCode){
        if (!globalsInited) InitGlobals();
        try{
            MainFunction = luaScript.LoadString(luaCode,null,scriptName);
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
    public Coroutine? RunningCoroutine;
    private Dictionary<Coroutine, DateTime> _sleepingCoroutines = new();
    public Dictionary<Coroutine, DynValue[]> CoroutinesArgs = new();
    public HashSet<Coroutine> coroutineDeclineArgs = new();
    private HashSet<Coroutine> _PausedCoroutines = new();
    public HashSet<Coroutine> PausedCoroutines{get=>_PausedCoroutines;}
    private CoreModules defaultModules = CoreModules.Preset_HardSandbox | CoreModules.Metatables |
            CoreModules.ErrorHandling | CoreModules.Coroutine | CoreModules.Json | CoreModules.OS_Time;
    public LuaConsole console;
    public LuaScript(){
        luaScript = new(defaultModules);
        scriptMethods = new(this);
        console = new(this);
        MakeGlobalENV();
        Task.Run(()=>{
            while (true){
                _runningEvent.Wait();
                if (isScrRunning){
                    DateTime? nearWakeUp = null;
                    Coroutine[] snapshot;
                    lock (_lock) snapshot = Coroutines.ToArray();
                    foreach (Coroutine coroutine in snapshot){
                        if (!isScrRunning) break;
                        bool Contains;
                        lock (_lock) Contains = Coroutines.Contains(coroutine);
                        if (Contains){
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
                                lock(_lock) if (!coroutineDeclineArgs.Contains(coroutine)){
                                    if (CoroutinesArgs.TryGetValue(coroutine,out var arg))
                                        args = arg;
                                    coroutineDeclineArgs.Add(coroutine);
                                } DynValue result; 
                                if (args!=null){
                                    result = coroutine.Resume(args);
                                    lock(_lock)CoroutinesArgs.Remove(coroutine);
                                }else result = coroutine.Resume();
                                // coroutine exit handler
                                if (result.Type != DataType.YieldRequest){
                                    if (result.Type!=DataType.UserData||result.ToObject()!=scriptMethods){
                                        lock(_lock) Coroutines.Remove(coroutine);
                                    }else coroutineDeclineArgs.Remove(coroutine);
                                }
                            }catch(ScriptRuntimeException exc){
                                lock(_lock) Coroutines.Remove(coroutine);
                                console.printException(exc);
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
        _newCoroutineStarted.Set();
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
    public ScriptMethods scriptMethods;
    private void MakeGlobalENV(){
        luaScript.Globals["print"] = (Action<object?>)((o)=>{
            console.rawPrint((o??"nil").ToString()??"null");
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
            lock (_lock)Coroutines.Add(coroutine.Coroutine);
            _newCoroutineStarted.Set();
        });
        luaScript.Globals["UDim"] = new UDim_Creator(luaScript);
        luaScript.Globals["PixelsSize"] = new PixelsSize_Creator(luaScript);
        luaScript.Globals["UDim2"] = new UDim2_Creator(luaScript);
        luaScript.Globals["script"] = scriptMethods;
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
        public void trigger(object[]? args)
        {
            if (args==null){trigger();return;}
            List<DynValue> objects = [];
            foreach (var obj in args)objects.Add(DynValue.FromObject(script.luaScript,obj));
            trigger(objects.ToArray());
        }
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
    public static string GetLuaType(Type t){
        var luaType = GetLuaTypeName(t);
        if (luaType!="userdata")return luaType;
        return GetLuaClassName(t);
    }
    public static Dictionary<Type,string> typesNameInLua = new(){
        [typeof(LuaEvent)]="Event",
        [typeof(ScriptMethods)]="Script",
        [typeof(LuaPermissions)]="Permissions",
        [typeof(ScriptWindow)]="Window",
        [typeof(ScriptMethods.LuaTrayMenu)]="TrayButton",
        [typeof(CSharpLuaError)]="error",
        [typeof(UDim)]="UDim",
        [typeof(UDim2)]="UDim2",
        [typeof(PixelsSize)]="PixelsSize",
        [typeof(UDim_Creator)]="UDimCreator",
        [typeof(PixelsSize_Creator)]="PixelsSizeCreator",
        [typeof(LuaProgramSync)]="Roblox",
        [typeof(LuaLogReader)]="LogReader",
        [typeof(JoinedGameData)]="JoinedGameData",
        [typeof(LogLine)]="LogLine",
        [typeof(LuaPromiseReturn)]="PromiseReturn",
    }; public static string GetLuaClassName(Type t){
        if (typesNameInLua.TryGetValue(t,out var typeName)) return typeName;
        return "unknown";
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