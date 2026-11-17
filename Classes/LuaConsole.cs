using MoonSharp.Interpreter;

namespace RoHelper;

class LuaConsole
{
    public static bool doHideGlobals = false;
    private LuaScript luaScript;
    public LuaConsole(LuaScript luaScript){
        this.luaScript = luaScript;
    }
    public void rawPrint(string str){
        Console.WriteLine(luaScript.scriptName+": "+str);
    }
    public void rawError(string str){
        Console.WriteLine(str);
    }
    public string getTraceback(IList<MoonSharp.Interpreter.Debugging.WatchItem> items){
        var trace = "Traceback:";
        foreach (var item in items){
            var itrace = "\n   ";
            var Location = item.Location;
            if (Location!=null){
                if (Location.IsClrLocation){
                    itrace += "<root in '"+item.Name+"'>";
                }else{
                    var scr = luaScript.luaScript.GetSourceCode(Location.SourceIdx);
                    if (scr.Name=="_GLOBALS"&&doHideGlobals)
                        continue;
                    itrace += scr.Name;
                    if (item.Name!=null)
                        itrace += " (in function '"+item.Name+"')";
                    itrace += " in line: "+Location.FromLine;
                    if (Location.FromLine!=Location.ToLine) itrace += "-"+Location.ToLine;
                }
            }else{
                itrace += "<unknown in '"+item.Name+"'>";
            } trace += itrace;
        } return trace;
    }
    public void printTraceback(IList<MoonSharp.Interpreter.Debugging.WatchItem> items){
        Console.WriteLine(getTraceback(items));
    }
    public void printException(ScriptRuntimeException exception){
        rawError(exception.DecoratedMessage);
        printTraceback(exception.CallStack);
    }
}