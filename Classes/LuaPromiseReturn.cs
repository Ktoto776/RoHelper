using MoonSharp.Interpreter;

namespace RoHelper;

[MoonSharpUserData]
class LuaPromiseReturn{
    private readonly object _lock = new object();
    [MoonSharpHidden]
    public Dictionary<LuaScript,HashSet<Coroutine>> pausedCoroutines = new();
    public DynValue[]? Connect(ScriptMethods methods){
        lock(_lock){
            if (resDArgs!=null) return resDArgs;
            if (resOArgs!=null) return getValuesFromObjects(methods.script.luaScript,resOArgs);
        } HashSet<Coroutine> coroutines;
        if (pausedCoroutines.TryGetValue(methods.script,out var rcoroutines))
            coroutines = rcoroutines;
        else{
            coroutines = new();
            lock(_lock) pausedCoroutines.Add(methods.script,coroutines);
        } var RunningCoroutine = methods.script.RunningCoroutine;
        if (RunningCoroutine!=null){
            methods.script.PausedCoroutines.Add(RunningCoroutine);
            lock(_lock) coroutines.Add(RunningCoroutine);
        } return null;
    }
    [MoonSharpHidden]
    public DynValue[]? resDArgs;
    [MoonSharpHidden]
    public object[]? resOArgs;
    [MoonSharpHidden]
    public DynValue[] getValuesFromObjects(Script luaScript,object[] objs){
        List<DynValue> dargs = new();
        foreach (var arg in objs) 
            dargs.Add(DynValue.FromObject(luaScript,arg));
        return dargs.ToArray();
    }
    [MoonSharpHidden]
    public void Return(DynValue[] args){
        lock(_lock){
            resDArgs = args;
            foreach (var pausedScr in pausedCoroutines){
                var luaScript = pausedScr.Key;
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
                var luaScript = pausedScr.Key;
                var dargs = getValuesFromObjects(luaScript.luaScript,args);
                foreach (var pausedCoroutine in pausedScr.Value){
                    luaScript.CoroutinesArgs[pausedCoroutine] = dargs;
                    luaScript.PausedCoroutines.Remove(pausedCoroutine);
                } luaScript._eventPausedUnlock.Set();
            } pausedCoroutines.Clear();
        }
    }
    [MoonSharpHidden]
    public void Clear(){
        lock(_lock){
            resDArgs = null;
            resOArgs = null;
        }
    }
}