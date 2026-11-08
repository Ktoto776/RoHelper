namespace RoHelper;

using System.Text.Json;
using System.Reflection;

class Translator
{
    private List<Action> Binded = [];
    private Assembly assembly = Assembly.GetExecutingAssembly();
    private Dictionary<string, string> json;
    private Dictionary<string, string> defaultDict = new Dictionary<string, string>();
    public Translator()
    {
        using Stream? stream = assembly.GetManifestResourceStream("RoHelper.translations.en.json");
        if (stream != null)
        {
            defaultDict = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)??defaultDict;
        }
        json = defaultDict;
    }
    
    public void bindToTranslateChanged(string name,Action<string> action,string? default_val =null)
    {
        Action lambda = () =>
            action(getTranslated(name,default_val));
        Binded.Add(lambda);
        lambda();
    }
    public void bindToTranslator(Action action)
    {
        Binded.Add(action);
        action();
    }
    public string getTranslated(string name,string? default_val =null)
    {
        if (json.TryGetValue(name,out string? data)){
            return data;
        }else{
            return default_val??name;
        }
    }
    public string langCode = "";
    public void setLanguage(string langCode ="en")
    {
        using var stream = assembly.GetManifestResourceStream($"RoHelper.translations.{langCode}.json");
        if (stream != null)
        {
            json = JsonSerializer.Deserialize<Dictionary<string, string>>(stream)??defaultDict;
        }else if (defaultDict != null)
        {
            json = defaultDict;
        }
        this.langCode = langCode;
        foreach (Action action in Binded){
            action();
        }
    }
}