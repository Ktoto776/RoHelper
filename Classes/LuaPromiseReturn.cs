using MoonSharp.Interpreter;

namespace RoHelper;

[MoonSharpUserData]
class LuaPromiseReturn{
    private readonly object _lock = new object();
    public Dictionary<LuaScript,HashSet<Coroutine>> pausedCoroutines = new();
    public void Connect(ScriptMethods methods){
        HashSet<Coroutine> coroutines;
        if (pausedCoroutines.TryGetValue(methods.script,out var rcoroutines))
            coroutines = rcoroutines;
        else{
            coroutines = new();
            pausedCoroutines.Add(methods.script,coroutines);
        } var RunningCoroutine = methods.script.RunningCoroutine;
        if (RunningCoroutine!=null){
            methods.script.PausedCoroutines.Add(RunningCoroutine);
            lock(_lock)coroutines.Add(methods.script.RunningCoroutine);
        }
    }
    public DynValue[]? resDArgs;
    public object[]? resOArgs;
    [MoonSharpHidden]
    public DynValue[] getValuesFromObjects(Script luaScript,object[] objs){
        List<DynValue> dargs = new();
        foreach (var arg in args) 
            dargs.Add(DynValue.FromObject(luaScript.luaScript,arg));
        return dargs.ToArray();
    }
    [MoonSharpHidden]
    public void Return(DynValue[] args){
        lock(_lock){
            resDArgs = args;
            foreach (var pausedScr in pausedCoroutines){
                var luaScript = pausedCoroutine.Key;
                foreach (var pausedCoroutine in pausedScr.Value){
                    luaScript.CoroutinesArgs[pausedCoroutine] = args;
                    luaScript.PausedCoroutines.Remove(pausedCoroutine);
                } luaScript._eventPausedUnlock.Set();
            }
        }
    }
    [MoonSharpHidden]
    public void Return(object[] args){
        lock(_lock){
            resOArgs = args;
            foreach (var pausedScr in pausedCoroutines){
                var luaScript = pausedCoroutine.Key;
                var dargs = getValuesFromObjects(luaScript.luaScript,args);
                foreach (var pausedCoroutine in pausedScr.Value){
                    luaScript.CoroutinesArgs[pausedCoroutine] = dargs;
                    luaScript.PausedCoroutines.Remove(pausedCoroutine);
                } luaScript._eventPausedUnlock.Set();
            }
        }
    }
}