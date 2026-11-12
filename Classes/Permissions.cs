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
    public bool isPermissionGranted(string permission){
        return GrantedPermissions.Contains(permission);
    } [MoonSharpHidden]
    static public HashSet<string> registeredPermissions =["create.window","create.tray_button"];
    [MoonSharpHidden]
    static void registerPermission(string permission){
        registeredPermissions.Add(permission);
    }
}