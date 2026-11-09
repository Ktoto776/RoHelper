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
    private HashSet<Coroutine> Coroutines = new() ;
    private ManualResetEventSlim _runningEvent = new(false);
    public ManualResetEventSlim _eventPausedUnlock = new(false);
    public ManualResetEventSlim _newCoroutineStarted = new(false);
    private Coroutine? RunningCoroutine;
    private Dictionary<Coroutine, DateTime> _sleepingCoroutines = new();
    private HashSet<Coroutine> _PausedCoroutines = new();
    public LuaScript(){
        MakeGlobalENV();
        Task.Run(()=>{
            while (true){
                _runningEvent.Wait();
                if (isScrRunning){
                    DateTime? nearWakeUp = null;
                    foreach (Coroutine coroutine in Coroutines.ToArray()){
                        if (isScrRunning&&Coroutines.Contains(coroutine)){
                            if (_sleepingCoroutines.TryGetValue(coroutine, out var resumeTime)){
                                if (DateTime.UtcNow < resumeTime) {
                                    if (nearWakeUp==null||nearWakeUp>resumeTime) nearWakeUp = resumeTime;
                                    continue;
                                } _sleepingCoroutines.Remove(coroutine);
                            } coroutine.AutoYieldCounter = DefaultAutoYieldCounter;
                            RunningCoroutine = coroutine;
                            try{
                                DynValue result = coroutine.Resume();
                                if (result.Type != DataType.YieldRequest){
                                    Coroutines.Remove(coroutine);
                                }
                            }catch(ScriptRuntimeException exc){
                                Console.WriteLine("LuaScript error: "+exc.DecoratedMessage);
                                Coroutines.Remove(coroutine);
                            }
                        }
                    } RunningCoroutine = null;
                    var pausedCount = _sleepingCoroutines.Count+_PausedCoroutines.Count;
                    if (Coroutines.Count==pausedCount){
                        _newCoroutineStarted.Reset();
                        _eventPausedUnlock.Reset();
                        if (nearWakeUp!=null){
                            TimeSpan time = (TimeSpan)(nearWakeUp-DateTime.UtcNow);
                            if (time>TimeSpan.FromMilliseconds(0)){
                                _eventPausedUnlock.Wait(time);
                            }else Thread.Sleep(0);
                        }else if (_PausedCoroutines.Count!=0) _eventPausedUnlock.Wait();
                        else if (Coroutines.Count==0) _newCoroutineStarted.Wait();
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
        Coroutines.Add(coroutine.Coroutine);
        _runningEvent.Set();
    }
    public event EventHandler? scriptStopped;
    public void StopScript(){
        if (!isScrRunning) return;
        isScrRunning = false;
        _runningEvent.Reset();
        Coroutines = new();
        _sleepingCoroutines = new();
        _PausedCoroutines = new();
        skipThisCoroutine();
        scriptStopped?.Invoke(this,EventArgs.Empty);
    }
    public ScriptMethods scriptMethods = new();
    private void MakeGlobalENV(){
        luaScript.Globals["wait"] = (Action<double>)((s)=>{
            var coroutine = RunningCoroutine;
            if (coroutine==null){
                Thread.Sleep(TimeSpan.FromSeconds(s));
                return;
            } coroutine.AutoYieldCounter = 1;
            if (s > 0){
                var time = DateTime.UtcNow.AddSeconds(s);
                _sleepingCoroutines[coroutine] = time;
            }
        });
        luaScript.Globals["spawn"] = (Action<DynValue>)((func) =>{
            DynValue coroutine = luaScript.CreateCoroutine(func);
            Coroutines.Add(coroutine.Coroutine);
            RunningCoroutine = coroutine.Coroutine;
            coroutine.Coroutine.AutoYieldCounter = DefaultAutoYieldCounter;
            coroutine.Coroutine.Resume();
        });
        luaScript.Globals["script"] = scriptMethods;
    }
    [MoonSharpUserData]
    public class LuaEvent{
        private LuaScript script;
        private HashSet<DynValue> ConnectedFuncs = new();
        private HashSet<Coroutine> OnceCoroutines = new();
        private HashSet<Coroutine> PausedCoroutines = new();
        private event EventHandler theEvent;
        public LuaEvent(LuaScript script){
            this.script = script;
            theEvent += (_,_)=>{
                foreach (var func in ConnectedFuncs){
                    var Coroutine = script.luaScript.CreateCoroutine(func);
                    script.Coroutines.Add(Coroutine.Coroutine);
                } foreach (var Coroutine in OnceCoroutines){
                    script.Coroutines.Add(Coroutine);
                } foreach (var Coroutine in PausedCoroutines){
                    script._PausedCoroutines.Remove(Coroutine);
                } if (PausedCoroutines.Count!=0){
                    PausedCoroutines = new();
                    script._eventPausedUnlock.Set();
                } if (ConnectedFuncs.Count!=0||OnceCoroutines.Count!=0){
                    script._eventPausedUnlock.Set();
                    script._newCoroutineStarted.Set();
                    OnceCoroutines = new();
                }
            }; //EventHandler
            script.scriptStopped += (_,_)=>{
                ConnectedFuncs = new();
                OnceCoroutines = new();
                PausedCoroutines = new();
            };
        } 
        [MoonSharpHidden]
        public void trigger(){
            theEvent?.Invoke(this,EventArgs.Empty);
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
                script._PausedCoroutines.Add(Running);
            }
        }
    }
}