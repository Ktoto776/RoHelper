using System.Globalization;
using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace RoHelper;

class LuaLogReader{
    public LuaScript.LuaEvent? NewRawLineEvent;
    public LuaScript.LuaEvent? NewLogLineEvent;
    public LuaScript.LuaEvent? Joined;
    public LuaScript.LuaEvent? Joining;
    public LuaScript.LuaEvent? Leaving;
    public LuaScript.LuaEvent? Disconnected;
    public LuaScript.LuaEvent? Telepoting;
    public LuaScript.LuaEvent? Teleported;
}

[MoonSharpUserData]
public class LogLine{
    [MoonSharpHidden]
    public bool isParsed = false;
    public string rawLine {get;private set;}
    public string TimeISO {get;private set;} ="";
    public double timestamp {get;private set;} =0;
    public double timeTag {get;private set;} = 0;
    public string contextId {get;private set;} = "<unknown>";
    public int logLevel {get;private set;} =0;
    public string logType {get;private set;} = "";
    public string Sender {get;private set;} ="";
    public string Data {get;private set;} ="";
    public LogLine(string raw){
        rawLine = raw;
        try{
            var SpaceData = raw.Split(@" [",2);
            var LogData = SpaceData[0].Split(",",5);
            TimeISO = LogData[0];
            try{
                if (DateTimeOffset.TryParse(TimeISO,out var date))
                    timestamp = date.ToUnixTimeMilliseconds()/1000.0;
                else return;
                if (double.TryParse(LogData[1],CultureInfo.InvariantCulture,out var ttag)){
                    timeTag = ttag;
                } contextId = LogData[2];
                if (int.TryParse(LogData[3], out var llevel)){
                    logLevel = llevel;
                }
                if (LogData.Length>=5) logType = LogData[4];
            }catch(Exception e){
                Console.WriteLine("Error when parse other log data: "+e.Message);
            } if (SpaceData.Length==1) return;
            var Data = SpaceData[1].Split("] ",2);
            Sender = Data[0];
            this.Data = Data[1];
            isParsed = true;
        }catch(Exception e){
            Console.WriteLine("Error when parse log line: "+e.Message);
        }
    }
}

class LogClientDisconnectedEventArgs(string res ="unknown") :EventArgs{
    public string reason =res;
}

class LogEventArgs :EventArgs{
    public LogLine line;
    public string rawLine;
    public LogEventArgs(string rawline,LogLine line){
        rawLine = rawline;
        this.line = line;
    }
}

static class LogReader{
    public static string LogsFolder = Path.Combine(AppData.AppDataPath,"Roblox","logs");
    public static string OldLogsFolder = Path.Combine(LogsFolder,"Old");
    public static int poolingDelay = 500;
    public static EventHandler<LogEventArgs>? logChanged;
    public static void StartReader(){
        LuaPermissions.registeredPermissions.Add("read_logs.all");
        LuaPermissions.registeredPermissions.Add("read_logs.join_event");
        LuaPermissions.registeredPermissions.Add("roblox.joined_server");
        LuaPermissions.registeredPermissions.Add("read_logs.left_event");
        LuaPermissions.registeredPermissions.Add("read_logs.teleport_events");
        Console.WriteLine(LogsFolder);
        try{
            Directory.CreateDirectory(OldLogsFolder);
        }catch{}
        if (Directory.Exists(OldLogsFolder)){
            foreach (string file in Directory.GetFiles(LogsFolder, "*.log")){
                try{
                    File.Move(file,Path.Combine(OldLogsFolder,file.Replace("/","\\").Split("\\").Last()));
                }catch{}
            }
        } Task.Run(MainPooling);
    }
    private static void MainPooling(){
        Dictionary<string,long> ReadPositions = new();
        while(true){
            foreach (string file in Directory.GetFiles(LogsFolder, "*.log")){
                var pos = ReadPositions.GetValueOrDefault(file,-1);
                var firstRead = pos==-1;
                try{
                    FileStream fs = new FileStream(file, FileMode.Open, FileAccess.Read, 
                        FileShare.ReadWrite);
                    if (fs.Length == pos) continue;
                    if (fs.Length<pos) firstRead = true;
                    if (!firstRead) fs.Seek(pos, SeekOrigin.Begin);
                    ReadPositions[file] = fs.Length;
                    using (StreamReader reader = new StreamReader(fs)){
                        string? line;
                        while ((line = reader.ReadLine()) != null){
                            whenNewLine(line,!firstRead);
                        }
                    }
                    fs.Dispose();
                }catch (IOException ex){
                    Console.WriteLine("Error when reading log: " + ex.Message);
                }
            }
            LogStackAnalizator.checkTimeouts();
            Thread.Sleep(poolingDelay);
        }
    }
    public static LuaLogReader createLogReaderEvents(ScriptMethods methods){
        LuaLogReader reader = new();
        if (methods.permissions==null) return reader;
        if (methods.permissions.isPermissionGranted("read_logs.all")){
            reader.NewLogLineEvent = new(methods.script);
            reader.NewRawLineEvent = new(methods.script);
            logChanged += (_,args)=>{
                if (args.line.isParsed)
                    reader.NewLogLineEvent.trigger(
                        [DynValue.FromObject(methods.script.luaScript,args.line)]
                    );
                reader.NewRawLineEvent.trigger(
                    [DynValue.NewString(args.rawLine)]
                );
            };
        }
        if (methods.permissions.isPermissionGranted("roblox.joined_server")
            ||methods.permissions.isPermissionGranted("read_logs.all"))
        {
            if (!methods.permissions.GrantedPermissions.Contains("read_logs.join_event"))
                methods.permissions.GrantedPermissions.Add("read_logs.join_event");
            if (!methods.permissions.GrantedPermissions.Contains("read_logs.left_event"))
                methods.permissions.GrantedPermissions.Add("read_logs.left_event");
            if (!methods.permissions.GrantedPermissions.Contains("read_logs.teleport_events"))
                methods.permissions.GrantedPermissions.Add("read_logs.teleport_events");
        }
        if (methods.permissions.isPermissionGranted("read_logs.join_event")){
            reader.Joining = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.Joined,(objs)=>reader.Joining.trigger(objs));
            reader.Joined = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.Joined,(objs)=>reader.Joined.trigger(objs));
        } if (methods.permissions.isPermissionGranted("read_logs.left_event")){
            reader.Leaving = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.Leaving,(objs)=>reader.Leaving.trigger(objs));
            reader.Disconnected = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.Disconnected,(objs)=>reader.Disconnected.trigger(objs));
        } if (methods.permissions.isPermissionGranted("read_logs.teleport_events")){
            reader.Telepoting = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.doingTeleport,(objs)=>reader.Telepoting.trigger(objs));
            reader.Teleported = new(methods.script);
            LogAnalizator.BindLuaEvent(LogAnalizator.AnalizeResultTypes.Teleported,(objs)=>reader.Teleported.trigger(objs));
        }
        return reader;
    }
    private static void whenNewLine(string rawLine,bool fireEvents){
        var line = new LogLine(rawLine);
        if (line.isParsed){
            var result = LogAnalizator.analizeLogLine(line);
            result.FireLuaEvents();
        }
        if (fireEvents){
            logChanged?.Invoke(rawLine,new LogEventArgs(rawLine,line));
        }
    }
    
}