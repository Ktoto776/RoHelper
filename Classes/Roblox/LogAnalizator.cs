using System.Text.RegularExpressions;
using MoonSharp.Interpreter;

namespace RoHelper;

public static class LogAnalizator{
    private static EventHandler<LEventArgs>? mainLuaEvent;
    private class LEventArgs{
        public AnalizeResultTypes type =0;
        public object[]? args;
    }
    public static void BindLuaEvent(AnalizeResultTypes type, Action<object[]?> func){
        mainLuaEvent += (_, e)=>{
            if (e.type==type){
                func(e.args);
            }
        };
    }
    public class AnalizeResult(AnalizeResultTypes type,object[]? eventArgs=null){
        public AnalizeResultTypes type = type;
        public bool isStackContinue = false;
        public object[]? args = eventArgs;
        public void FireLuaEvents(){
            var data = new LEventArgs();
            data.type = type;
            data.args = args;
            mainLuaEvent?.Invoke(this,data);
        }
    }
    public enum AnalizeResultTypes{
        Nothing = 0,
        RobloxPathRefreshed = 1,
        Joining=2,
        Joined=3,
        NewDeeplinkStack=4,
        DeeplinkLeft=5,
        DeeplinkJoin=6,
        Leaving=7,
        Disconnected=8,
        NewJoiningServerData=9
    }
    public static class Flags{
        public static bool isRobloxPathInited = false;
    }
    public const string robloxClientName = "RobloxPlayerBeta.exe";
    public static class Data{
        public static string robloxUpdaterPath = "";
        public static string robloxVersionPath = "";
        public static string robloxClientPath = "";
        public static string userId = "";
        public static class JoinedGame{
            public static bool isConnected =false;
            public static string JobId ="";
            public static string PlaceId = "";
            public static string RobloxServerIP ="";
            public static string ServerAddress ="";
            public static void Clear(){
                JobId ="";
                PlaceId = "";
                RobloxServerIP ="";
                ServerAddress = "";
            }
        }
    }
    public static class AnalyzeData{
        // Prefix for check updater path
        public const string UpdaterPathPrefix = "WindowsUpdateController: updaterFullPath: ";
        // Joining to game
        public static Regex JoinRegex = new(@"Joining game '([^']+)' place (\d+) at ([^\s]+)");
        public const string JoinedServerPrefix = "Connection accepted from ";
        public const string DisconnedPrefix = "Client has been disconnected with reason: ";
        //UserId
        public static Regex UserIdWhenJoinRegex = new(@"userid:(\d+)");
        public static Regex UserIdAltCookieRegex = new(@"rbxuid:(\d+)");
    }
    private static bool isJoinDataCollecting = false;
    public static AnalizeResult analizeLogLine(LogLine line){
        // Roblox Path (from updater path)
        if (line.Sender=="FLog::UpdateController"){
            if (line.Data.StartsWith(AnalyzeData.UpdaterPathPrefix)){
                var UpdaterPath = line.Data.Remove(0,AnalyzeData.UpdaterPathPrefix.Length);
                Flags.isRobloxPathInited = true;
                Data.robloxUpdaterPath = UpdaterPath;
                Data.robloxVersionPath = Path.GetDirectoryName(UpdaterPath)??"";
                Data.robloxClientPath = Path.Combine(Data.robloxVersionPath,robloxClientName);
                Console.WriteLine("Roblox version directory detected: "+Data.robloxVersionPath);
                return new AnalizeResult(AnalizeResultTypes.RobloxPathRefreshed);
            }
        }
        // Roblox Join to place
        if (line.Sender == "FLog::SingleSurfaceApp"){
            if (line.Data.Contains("launchUGCGameInternal")){
                Data.JoinedGame.Clear();
                var result = new AnalizeResult(AnalizeResultTypes.Joining);
                result.isStackContinue = LogStackAnalizator.analyzeFromType.analyzeJoin();
                return result;
            }
        }else if(line.Sender=="FLog::Output"){ 
            var JoinMatch = AnalyzeData.JoinRegex.Match(line.rawLine);
            if (JoinMatch.Success){
                Data.JoinedGame.JobId = JoinMatch.Groups[1].Value;
                Data.JoinedGame.PlaceId = JoinMatch.Groups[2].Value;
                Data.JoinedGame.RobloxServerIP = JoinMatch.Groups[3].Value;
                Data.JoinedGame.isConnected = true;
                return new AnalizeResult(AnalizeResultTypes.NewJoiningServerData);
            }
        }else if(line.Sender=="DFLog::NetworkClient"){ 
            if (line.Data.StartsWith(AnalyzeData.JoinedServerPrefix)){
                var ServerIPAndPort = line.Data.Remove(0,AnalyzeData.JoinedServerPrefix.Length)
                    .Replace("|",":");
                Data.JoinedGame.ServerAddress = ServerIPAndPort;
                return new AnalizeResult(AnalizeResultTypes.Joined);
            }
        }
        // Leaving
        if (line.Sender=="FLog::SingleSurfaceApp"){
            if (line.Data.Contains("leaveUGCGame")){
                AnalizeResult result = new(AnalizeResultTypes.Leaving);
                result.isStackContinue = LogStackAnalizator.analyzeFromType.analyzeLeave();
                Data.JoinedGame.isConnected = false;
                return result;
            }
        }
        // Disconnected
        if(line.Sender=="FLog::Network"){
            if (line.Data.StartsWith(AnalyzeData.DisconnedPrefix)){
                AnalizeResult result = new(AnalizeResultTypes.Disconnected,
                    [line.Data.Remove(0,AnalyzeData.DisconnedPrefix.Length)]);
                return result;
            }
        }
        // Getting UserId
        if (line.Sender=="FLog::GameJoinLoadTime"){
            Match match = AnalyzeData.UserIdWhenJoinRegex.Match(line.Data);
            if (match.Success) Data.userId = match.Groups[1].Value.Trim();
        }else if(line.Sender==""){
            Match match = AnalyzeData.UserIdAltCookieRegex.Match(line.Data);
            if (match.Success) Data.userId = match.Groups[1].Value.Trim();
        }
        // if(line.Sender=="DFLog::NetworkClient"&&line.Data.Contains("Client:Disconnect")){
        //     if (!JoinedGame.isConnected){
        //         if(JoinedGame.isDisconnected){
        //             user_left?.Invoke(line,EventArgs.Empty);
        //             JoinedGame.isDisconnected = false;
        //         } return;
        //     } JoinedGame.isConnected = false;
        //     user_success_left?.Invoke(line,EventArgs.Empty);
        // }else 
        return LogStackAnalizator.analyze(line) ??new AnalizeResult(AnalizeResultTypes.Nothing);
    }
}

public class JoinedGameData{
    public static bool isConnected{
        get=>LogAnalizator.Data.JoinedGame.isConnected;
    }
    public static string JobId{
        get=>LogAnalizator.Data.JoinedGame.JobId;
    }
    public static string PlaceId{
        get=>LogAnalizator.Data.JoinedGame.PlaceId;
    }
    public static string RobloxServerIP{
        get=>LogAnalizator.Data.JoinedGame.RobloxServerIP;
    }
    public static string ServerAddress{
        get=>LogAnalizator.Data.JoinedGame.ServerAddress;
    }
}