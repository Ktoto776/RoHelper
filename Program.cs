namespace RoHelper;

using System.Globalization;
using System.Reflection;
using MoonSharp.Interpreter;

static class Program
{
    public static Translator translator = new Translator();
    public static MainTray mainTray = null!;
    public static MainWindow mainWindow = null!;
    public static AppData appData = new AppData();
    [STAThread]
    static void Main()
    {
        // APP
        ApplicationConfiguration.Initialize();
        Application.SetColorMode(SystemColorMode.System);
        // Translator
        translator.setLanguage(CultureInfo.CurrentUICulture.TwoLetterISOLanguageName);
        // Tray
        mainTray = new MainTray();
        // RunWindow
        mainWindow = new MainWindow();
        // Addons
        UserData.RegisterType<ScriptMethods>();
        UserData.RegisterType<LuaScript.LuaEvent>();
        UserData.RegisterType<LuaSetting>();
        loadEmbeddedAddons();
        runLoadedAddons();
        Application.Run(mainWindow);
        appData.saveWindows();
    }
    const string EmbeddedAddonsPrefix = "RoHelper.Addons.Embedded.";
    const string EmbeddedAddonsSuffix = ".info.json";
    static void loadEmbeddedAddons(){
        Assembly assembly = Assembly.GetExecutingAssembly();

        var filtered = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(EmbeddedAddonsPrefix, StringComparison.OrdinalIgnoreCase)
                        && name.EndsWith(EmbeddedAddonsSuffix, StringComparison.OrdinalIgnoreCase))
            .ToArray();
        
        foreach (string infoPath in filtered){
            var path = infoPath.Replace(EmbeddedAddonsSuffix,".");
            Console.WriteLine("Loading embedded addon: "+path+"*");
            var addon = new Addon();
            using (Stream? stream = assembly.GetManifestResourceStream(infoPath))
            {
                if (stream != null){
                    try{
                        addon.loadInfoFromStream(stream,true);
                    }catch{
                        Console.WriteLine("Error to load "+path+"*");
                    }
                }
            }
            using (Stream? stream = assembly.GetManifestResourceStream(path+"script.lua"))
            {
                if (stream != null){
                    using StreamReader reader = new StreamReader(stream);
                    string content = reader.ReadToEnd();
                    addon.luaScript.Compile(content);
                }
            }
        }
    }
    private static void runLoadedAddons(){
        foreach (KeyValuePair<string,Addon> pair in Addon.loadedAddons){
            Addon addon = pair.Value;
            if (addon.addonIsLoaded()) Task.Run(addon.luaScript.RunScript);
        }
    }
}