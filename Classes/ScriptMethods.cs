using MoonSharp.Interpreter;
namespace RoHelper;


class ScriptMethods{
    public LuaScript.LuaEvent? addonSettingsChanged;
    public Dictionary<string,Dictionary<string,LuaSetting>>? settings;
}