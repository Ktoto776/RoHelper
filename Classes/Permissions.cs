using MoonSharp.Interpreter;

namespace RoHelper;

class LuaPermissions{
    [MoonSharpHidden]
    public HashSet<string> GrantedPermissions =new();
    [MoonSharpHidden]
    public HashSet<string> RequiredUserPermissions =new();
    [MoonSharpHidden]
    public HashSet<string> GrantedUserPermissions =new();
    public bool isPermissionGranted(DynValue? permission){
        if (permission!=null&&permission.Type==DataType.String){
            var permissionStr = permission.CastToString();
            return GrantedPermissions.Contains(permissionStr);
        }
        return false;
    }
    public Dictionary<string,LuaPromiseReturn> activePromptReturns = new();
    public HashSet<UserPermissionPrompt> activrPromots = new();
    public object PermissionRequest(){
        if (!RequiredUserPermissions.Contains())
            return DynValue.False;
        if (GrantedUserPermissions.Contains())
            return DynValue.True;
        LuaPromiseReturn ret = new();
        
        return ret;
    }
    public bool isUserPermissionGranted(string permission){
        return GrantedUserPermissions.Contains(permission);
    }
    public bool isPermissionGranted(string permission){
        return GrantedPermissions.Contains(permission);
    } [MoonSharpHidden]
    static public HashSet<string> registeredPermissions =["create.window","create.tray_button","os.time"];
    [MoonSharpHidden]
    static void registerPermission(string permission){
        registeredPermissions.Add(permission);
    }
}