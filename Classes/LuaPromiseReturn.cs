using MoonSharp.Interpreter;

namespace RoHelper;

[MoonSharpUserData]
class LuaPromiseReturn<T>{
    public Dictionary<LuaScript,HashSet<Coroutine>> pausedCoroutines = new();
    public void Connect(ScriptMethods methods){
        HashSet<Coroutine> coroutines;
        if (pausedCoroutines.TryGetValue(methods.script,out var rcoroutines))
            coroutines = rcoroutines;
        else{
            coroutines = new();
            pausedCoroutines.Add(methods.script,coroutines);
        } if (methods.script.RunningCoroutine!=null)
            coroutines.Add(methods.script.RunningCoroutine);
    }
    [MoonSharpHidden]
    public void Return(T data){
        
    }
}