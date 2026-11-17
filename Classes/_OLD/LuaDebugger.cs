using Microsoft.Win32.SafeHandles;
using MoonSharp.Interpreter;
using MoonSharp.Interpreter.Debugging;

namespace RoHelper;

public class StacktraceResult{
    // Can be "" = not in function
    public string FunctionName = "<unknown>";
    public string ScriptName = "<unknown>";
    public int? FromLine;
    public int? ToLine;
    public override string ToString()
    {
        string res = ScriptName+" ";
        if (FunctionName!="")
            res=res+"(in function '"+FunctionName+"') ";
        res = res+"[line: "+FromLine;
        if (FromLine==ToLine)
            res = res+"]";
        else res = res+"-"+ToLine+"]";
        return res;
    }
}

public class LuaDebuger : IDebugger
{
    private bool collectingStack = false;
    private bool doPause = false;
    private HashSet<Action<StacktraceResult[]>> stacktraces = new();
    public void getTraceback(Action<StacktraceResult[]> bind){
        doPause = true;
        collectingStack = true;
        stacktraces.Add(bind);
    }
    // DEBUGGER
    public DebuggerAction GetAction(int ip, SourceRef sourceref)
    {
        var action = new DebuggerAction();
        if (collectingStack) 
            action.Action = DebuggerAction.ActionType.StepOut;
        else action.Action = DebuggerAction.ActionType.Run;
        return action;
    }

    public DebuggerCaps GetDebuggerCaps()
    {
        return 0;
    }

    public List<DynamicExpression> GetWatchItems()
    {
        return new();
    }

    public bool IsPauseRequested()
    {
        if (doPause){
            doPause = false;
            return true;
        } return false;
    }

    public void RefreshBreakpoints(IEnumerable<SourceRef> refs)
    {}

    public void SetByteCode(string[] byteCode)
    {}

    public void SetDebugService(DebugService debugService)
    {}
    public Dictionary<int,string> ScriptNames = new();
    public void SetSourceCode(SourceCode sourceCode)
    {
        var Name = sourceCode.Name;
        if (sourceCode.Name=="_GLOBALS")
            Name = "<global>";
        ScriptNames.Add(sourceCode.SourceID,Name);
    }

    public void SignalExecutionEnded()
    {}

    public bool SignalRuntimeException(ScriptRuntimeException ex)
    {
        collectingStack = true;
        doPause = true;
        return true;
    }
    public List<StacktraceResult> stacktrace = new();
    public void Update(WatchType watchType, IEnumerable<WatchItem> items, int stackFrameIndex)
    {  
        if (watchType != WatchType.CallStack||!collectingStack)
            return;
        stacktrace.Clear();
        foreach (var item in items){
            var stackRes = new StacktraceResult();
            stackRes.FunctionName = item.Name ?? "";
            if (item.Location!=null){
                if (item.Location.IsClrLocation){
                    stackRes.ScriptName = "<root>";
                }else{
                    stackRes.FromLine = item.Location.FromLine;
                    stackRes.ToLine = item.Location.ToLine;
                    stackRes.ScriptName = ScriptNames[item.Location.SourceIdx] ?? "<unknown>";
                }
            }
            stacktrace.Add(stackRes);
        } collectingStack = false;
    }
}