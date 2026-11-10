using MoonSharp.Interpreter;
namespace RoHelper;

[MoonSharpUserData]
class ScriptMethods{
    public LuaScript.LuaEvent? addonSettingsChanged;
    public Dictionary<string,Dictionary<string,LuaSetting>>? settings;
}