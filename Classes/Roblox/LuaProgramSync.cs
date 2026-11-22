using System.Diagnostics;
using MoonSharp.Interpreter;

namespace RoHelper;

[MoonSharpUserData]
class LuaProgramSync{
    private ScriptMethods scriptMethods;
    public LuaLogReader logReader;
    public JoinedGameData? _joinedGameData = null;
    public JoinedGameData? joinedGameData{get=>_joinedGameData;}
    public LuaProgramSync(ScriptMethods methods){
        scriptMethods = methods;
        logReader = LogReader.createLogReaderEvents(methods);
        if (methods.permissions!=null){
            if (methods.permissions.isPermissionGranted("read_logs.all")){
                methods.permissions.GrantedPermissions.Add("roblox.joined_server");
            }
            if (methods.permissions.isPermissionGranted("roblox.joined_server")){
                _joinedGameData = new();
            }
        }
    }
    public string? UserId{
        get{
            if (scriptMethods.permissions==null||
                !scriptMethods.permissions.isPermissionGranted("roblox.userId"))
                    return null;
            else return LogAnalizator.Data.userId;
        }
    }
    public DynValue joinToServer(string placeId,string? JobID){
        if (scriptMethods.permissions==null||
            !scriptMethods.permissions.isUserPermissionGranted("roblox.join_game"))
            return DynValue.FromObject(scriptMethods.script.luaScript,new CSharpLuaError("Permission is not granted"));
        if (!LogAnalizator.Flags.isRobloxPathInited) return DynValue.False;
        var deeplink = $"roblox://experiences/start?placeId={placeId}";
        if (JobID!=null)
            deeplink += $"&serverId={JobID}";
        Process.Start(LogAnalizator.Data.robloxClientPath, deeplink);
        return DynValue.True;
    }
    [MoonSharpHidden]
    public static void _initClass(){
        LuaPermissions.registeredPermissions.Add("roblox.join_game");
    }
}