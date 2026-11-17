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
}