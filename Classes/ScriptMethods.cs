using MoonSharp.Interpreter;
namespace RoHelper;

class ScriptMethods{
    public LuaScript.LuaEvent? addonSettingsChanged;
    public Dictionary<string,Dictionary<string,LuaSetting>>? settings;
    public LuaPermissions? permissions;
    [MoonSharpHidden]
    public LuaScript script;
    [MoonSharpHidden]
    public LuaTrayMenu? luaTrayMenu;
    private HashSet<ScriptWindow> scriptWindows =new();
    private Addon? addon;
    private void bindLTMToAddonName(){
        if (luaTrayMenu!=null){
            if (addon!=null)
                Program.translator.bindToTranslator(()=>{
                    luaTrayMenu.stripItem.Text = addon.addonDisplayName;
                });
            else luaTrayMenu.stripItem.Text = script.scriptName;
        }
    }
    [MoonSharpHidden]
    public void setAddon(Addon addon){
        this.addon = addon;
        bindLTMToAddonName();
    }
    public LuaTrayMenu? getTrayMenuButton(){
        if (permissions==null||!permissions.isPermissionGranted("create.tray_button")) return null;
        if (luaTrayMenu!=null) return luaTrayMenu;
        luaTrayMenu = new(this);
        bindLTMToAddonName();
        return luaTrayMenu;
    }
    public ScriptWindow? createWindow(){
        if (permissions==null||!permissions.isPermissionGranted("create.window")) return null;
        var scriptWindow = new ScriptWindow(this);
        scriptWindow.Destroying.theEvent += (_,_)=>
            scriptWindows.Remove(scriptWindow);
        scriptWindows.Add(scriptWindow);
        return scriptWindow;
    }
    public ScriptMethods(LuaScript script){
        this.script = script;
        script.scriptStopped += (_,_)=>{
            foreach (var window in scriptWindows) window.Destroy();
            scriptWindows = new();
            if (luaTrayMenu!=null) luaTrayMenu.Destroy();
        };
    }
    public class LuaTrayMenu{
        [MoonSharpHidden]
        public ToolStripMenuItem stripItem =new();
        private ScriptMethods methods;
        public LuaScript.LuaEvent Clicked;
        public void Destroy(){
            methods.luaTrayMenu = null;
            Program.mainTray.trayMenu.Items.Remove(stripItem);
        }
        public LuaTrayMenu(ScriptMethods methods){
            this.methods = methods;
            Clicked = new(methods.script);
            stripItem.Text = "_";
            Program.mainTray.trayMenu.Items.Insert(0,stripItem);
            stripItem.Click += (_,_)=>{
                Clicked.trigger();
            };
        }
    }
}