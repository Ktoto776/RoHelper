using Microsoft.VisualBasic.ApplicationServices;
using MoonSharp.Interpreter;

namespace RoHelper;

class LuaPermissions{
    // [MoonSharpHidden]
    private LuaScript luaScript;
    public LuaPermissions(LuaScript luaScript){
        this.luaScript = luaScript;
        luaScript.scriptStopped += (_,_)=>{
            foreach (var window in activePromots){
                Program.mainWindow.Invoke(window.form.Close);
            } activePromots.Clear();
            activePromptReturns.Clear();
        };
    }
    [MoonSharpHidden]
    public HashSet<string> GrantedPermissions =new();
    [MoonSharpHidden]
    public HashSet<string> RequiredUserPermissions =new();
    [MoonSharpHidden]
    public HashSet<string> GrantedUserPermissions =new();
    [MoonSharpHidden]
    public HashSet<string> OnceGrantedUserPermissions =new();
    public bool isPermissionGranted(DynValue? permission){
        if (permission!=null&&permission.Type==DataType.String){
            var permissionStr = permission.CastToString();
            return GrantedPermissions.Contains(permissionStr);
        }
        return false;
    }
    public Dictionary<string,LuaPromiseReturn> activePromptReturns = new();
    public HashSet<UserPermissionPrompt> activePromots = new();
    public object PermissionRequest(string userPermission){
        if (!RequiredUserPermissions.Contains(userPermission))
            return DynValue.False;
        if (GrantedUserPermissions.Contains(userPermission))
            return DynValue.True;
        if (activePromptReturns.TryGetValue(userPermission,out var promiseReturn))
            return promiseReturn;
        LuaPromiseReturn ret = new();
        activePromptReturns[userPermission] = ret;
        UserPermissionPrompt prompt = new(luaScript);
        activePromots.Add(prompt);
        prompt.UserPermissionName = userPermission;
        prompt.form.FormClosed += (_,_)=>{
            activePromots.Remove(prompt);
            activePromptReturns.Remove(userPermission);
        };
        prompt.form.AcceptOnce +=(_,_)=>{
            GrantedUserPermissions.Add(userPermission);
            OnceGrantedUserPermissions.Add(userPermission);
            ret.Return([DynValue.True]);
        };
        prompt.form.Accept +=(_,_)=>{
            GrantedUserPermissions.Add(userPermission);
            ret.Return([DynValue.True]);
        };
        prompt.form.Decline +=(_,_)=>{
            ret.Return([DynValue.False]);
        };
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