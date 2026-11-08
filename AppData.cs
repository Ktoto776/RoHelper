using System.Text.Json;

namespace RoHelper;

class AppData_WindowSave
{
    public int X { get; set; } = 0;
    public int Y { get; set; } = 0;
    public int Width { get; set; } = 0;
    public int Height { get; set; } = 0;
}

class AppData
{
    static string AppDataPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
    string AppFolder = Path.Combine(AppDataPath, "RoHelper");
    public Dictionary<string,AppData_WindowSave> WindowData = new Dictionary<string, AppData_WindowSave>();
    public AppData()
    {
        Directory.CreateDirectory(AppFolder);
        string? json = loadTextFile("windowSettings.json");
        if (json!=null)
        {
            WindowData = JsonSerializer.Deserialize<Dictionary<string,AppData_WindowSave>>(json)??WindowData;
        }
    }
    public string? loadTextFile(string fileName)
    {
        string path = Path.Combine(AppFolder,fileName);
        if (File.Exists(path))
        {
            return File.ReadAllText(path);
        }
        return null;
    }
    public void saveTextFile(string fileName,string value)
    {
        string path = Path.Combine(AppFolder,fileName);
        File.WriteAllText(path, value);
    }
    public void saveWindows()
    {
        string json = JsonSerializer.Serialize(WindowData);
        saveTextFile("windowSettings.json",json);
    }
    public AppData_WindowSave? getWindowData(string WindowName)
    {
        if (Program.appData.WindowData.TryGetValue(WindowName,out AppData_WindowSave? data)){
            return data;
        }else{
            return null;
        }
    }
}