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
    private HashSet<Coroutine> Coroutines = new();
    private ManualResetEventSlim _runningEvent = new(false);
    private Coroutine? RunningCoroutine;
    private Dictionary<Coroutine, DateTime> _sleepingCoroutines = new();
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
                    if (nearWakeUp!=null&&Coroutines.Count==_sleepingCoroutines.Count){
                        TimeSpan time = (TimeSpan)(nearWakeUp-DateTime.UtcNow);
                        if (time>TimeSpan.FromMilliseconds(0)){
                            Thread.Sleep(time);
                        }else Thread.Sleep(0);
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
    public void StopScript(){
        if (!isScrRunning) return;
        isScrRunning = false;
        _runningEvent.Reset();
        Coroutines = new();
        _sleepingCoroutines = new();
        skipThisCoroutine();
    }
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
    }
}