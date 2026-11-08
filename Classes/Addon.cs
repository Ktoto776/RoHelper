using System.Reflection;
using System.Text.Json;
using System.Windows.Forms.VisualStyles;
using MoonSharp.Interpreter;

namespace RoHelper;

class ScriptStopException : Exception
{
    public ScriptStopException(string reason) : base(reason){}
}

class Addon{
    private string theAddonName = "";
    private bool isLoadedPrivate = false;
    public bool isEmbedded = false;
    public int versionBuild = 0;
    public string versionName = "-";
    public bool isLoaded{
        get=>isLoadedPrivate;
    }
    public string addonName{
        get=>theAddonName;
        set{theAddonName=value; if (theAddonDisplayName=="") setDisplayName(value); loadedCheck();}
    }
    private string theAddonDisplayName = "";
    public string addonDisplayName{
        get{
            if (theAddonDisplayName=="") return theAddonName;
            else return theAddonDisplayName;
        } set{theAddonDisplayName = value; setDisplayName(value);}
    }
    private void setDisplayName(string dName){
        SettingsButton.Text = dName;
        settingsPanel.Name = dName;
    }
    public static Dictionary<string,Addon> loadedAddons = new();
    private void loadedCheck(){
        isLoadedPrivate = theAddonName!="";
        if (isLoadedPrivate) if(loadedAddons.TryGetValue(addonName,out var addon)){
            if (addon.versionBuild<versionBuild){
                loadedAddons[addonName] = this;
            }
        }else loadedAddons[addonName] = this;
    }
    public bool addonIsLoaded(){
        if (isLoadedPrivate&&luaScript.isCompiled){
            SettingsButton.Visible = isLoadedPrivate;
            return true;
        } return false;
    }
    public Button SettingsButton;
    public SettingsPanel settingsPanel;
    public SettingsPage settingsPage;
    public LuaScript luaScript = new();
    public Addon(){
        SettingsButton = Program.mainWindow.createLeftItemButton();
        Program.mainWindow.leftAddonPanel.Controls.Add(SettingsButton);
        settingsPanel = Program.mainWindow.createSettingsPanel();
        settingsPage = settingsPanel.settingsPage;
        // Functions
        SettingsButton.Click += (_,_)=>settingsPanel.Activate();
        loadedCheck();
    }
    public void loadInfo(AddonJsonInfo info,bool isEmbedded =false){
        this.isEmbedded = isEmbedded;
        var Name = info.Name;
        versionBuild = info.versionBuild;
        if (info.versionName!=null) versionName = info.versionName;
        if (isEmbedded) Name = "_"+Name+"_";
        else Name = Name.Replace("_","__");
        addonName = Name;
        if (info.DisplayName!=null){
            Program.translator.bindToTranslator(()=>{
                var langCode = Program.translator.langCode;
                if (info.DisplayName.ContainsKey(langCode))
                    addonDisplayName=info.DisplayName[langCode];
                else addonDisplayName=info.Name;
            });
        }
    }
    public Exception? InfoJSONExcepltion;
    public void loadInfoFromStream(Stream stream,bool isEmbedded =false){
        try{
            var info = JsonSerializer.Deserialize<AddonJsonInfo>(stream);
            if (info!=null) loadInfo(info,isEmbedded);
        }catch(Exception e){
            InfoJSONExcepltion = e;
            Console.WriteLine("Error to read info.json: "+e.Message);
        }
    }
}

class AddonJsonInfo{
    public string Name {get;set;} = "";
    public Dictionary<string,string>? DisplayName {get;set;}
    public string? versionName {get;set;}
    public int versionBuild {get;set;} = 0;
    public Dictionary<string,AddonSettingsGroupJsonInfo>? Settings {get;set;}
}

class AddonSettingsGroupJsonInfo{
    public Dictionary<string,string>? groupDisplayName {get;set;}
    public Dictionary<string,AddonSettingJsonInfo> settings = new();
}

class AddonSettingJsonInfo{
    public Dictionary<string,string>? DisplayName {get;set;}
    public string type = "bool";
}