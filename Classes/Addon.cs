using System.Data.Common;
using System.Reflection;
using System.Runtime.InteropServices.JavaScript;
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
    public LuaScript.LuaEvent settingsChanged;
    public void addonSettingsChanged(){
        settingsChanged.trigger();
    }
    public Addon(){
        SettingsButton = Program.mainWindow.createLeftItemButton();
        Program.mainWindow.leftAddonPanel.Controls.Add(SettingsButton);
        settingsPanel = Program.mainWindow.createSettingsPanel();
        settingsPage = settingsPanel.settingsPage;
        // Functions
        SettingsButton.Click += (_,_)=>settingsPanel.Activate();
        loadedCheck();
        // Events
        settingsChanged = new(luaScript);
        luaScript.scriptMethods.addonSettingsChanged = settingsChanged;
        luaScript.scriptMethods.settings = luaSettings;
    }
    public Dictionary<string,SettingsPageGroup> settingsGroups = new();
    public Dictionary<string,Dictionary<string,LuaSetting>> luaSettings = new();
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
        if (info.Settings!=null){
            foreach (var Group in info.Settings){
                var name = Group.Key;
                var GroupData = Group.Value;
                var sGroup = settingsPage.createGroup();
                if (GroupData.groupDisplayName!=null){
                    Program.translator.bindToTranslator(()=>{
                        var langCode = Program.translator.langCode;
                        if (GroupData.groupDisplayName.TryGetValue(langCode,out var translate))
                            sGroup.HeaderText=translate;
                        else sGroup.HeaderText=name;
                    });
                }else sGroup.HeaderText=name;
                Dictionary<string,luaSetting> luaSGroup = new();
                foreach (var setting in GroupData.settings){
                    var sdata = setting.Value;
                    var type = sdata.type.Replace("-","").ToLower();
                    var withMinus = sdata.type.Length > 0 && sdata.type[0] == '-';
                    var defVal = sdata.defaultValue;
                    dynamic baseSetting = null!;
                    if (type=="int"){
                        var bsetting = sGroup.createIntSetting();
                        bsetting.canBeNegative = withMinus;
                        bsetting.defaultValue = JSONTypeGetter.getInt(defVal) ?? bsetting.defaultValue;
                        baseSetting = bsetting;
                    }else if (type=="float"){
                        var bsetting = sGroup.createFloatSetting();
                        bsetting.canBeNegative = withMinus;
                        bsetting.defaultValue = JSONTypeGetter.getFloat(defVal) ?? bsetting.defaultValue;
                        baseSetting = bsetting;
                    }else if (type=="double"){
                        var bsetting = sGroup.createDoubleSetting();
                        bsetting.canBeNegative = withMinus;
                        bsetting.defaultValue = JSONTypeGetter.getDouble(defVal) ?? bsetting.defaultValue;
                        baseSetting = bsetting;
                    }else if (type=="string"){
                        var bsetting = sGroup.createStringSetting();
                        bsetting.defaultValue = JSONTypeGetter.getString(defVal) ?? bsetting.defaultValue;
                        baseSetting = bsetting;
                    }
                    if (baseSetting!=null){
                        if (sdata.DisplayName!=null){
                            Program.translator.bindToTranslator(()=>{
                                var langCode = Program.translator.langCode;
                                if (sdata.DisplayName.TryGetValue(langCode,out var translate))
                                    baseSetting.NameText=translate;
                                else baseSetting.NameText=setting.Key;
                            });
                        }else baseSetting.NameText=setting.Key;
                        dynamic luaSetting = new LuaSetting(luaScript,baseSetting);
                        luaSGroup[setting.Key] = luaSetting;
                        EventHandler ValueChanged = (_,_)=>{
                            addonSettingsChanged();
                            luaSetting.onValueChanged.trigger();
                        };
                        baseSetting.ValueChanged += ValueChanged;
                    }
                }
                luaSettings[name] = luaSGroup;
                //addonSettingsChanged
                settingsGroups[name] = sGroup;
            }
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
static class JSONTypeGetter{
    static public int? getInt(JsonElement? element){
        if (element==null) return null;
        if (element?.ValueKind==JsonValueKind.Number){
            if (element?.TryGetInt32(out var res) ?? false) return res;
        } return null;
    }
    static public double? getDouble(JsonElement? element){
        if (element==null) return null;
        if (element?.ValueKind==JsonValueKind.Number){
            return element?.GetDouble();
        } return null;
    }
    static public float? getFloat(JsonElement? element){
        if (element==null) return null;
        if (element?.ValueKind==JsonValueKind.Number){
            return element?.GetSingle();
        } return null;
    }
    static public string? getString(JsonElement? element){
        if (element==null) return null;
        if (element?.ValueKind==JsonValueKind.String){
            return element?.GetString();
        } return null;
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
    public Dictionary<string,AddonSettingJsonInfo> settings {get;set;} = new();
}

class AddonSettingJsonInfo{
    public Dictionary<string,string>? DisplayName {get;set;}
    public string type {get;set;} = "bool";
    public JsonElement? defaultValue {get;set;}
}

[MoonSharpUserData]
class LuaSetting{
    private BaseSetting<object> baseSetting;
    public object value{
        get=>baseSetting.value;
        set=>baseSetting.value=value;
    }
    public LuaScript.LuaEvent onValueChanged;
    public LuaSetting(LuaScript script,BaseSetting<object> setting){
        baseSetting = setting;
        onValueChanged = new(script);
    }
}