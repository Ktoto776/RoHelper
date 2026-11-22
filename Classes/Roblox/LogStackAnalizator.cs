namespace RoHelper;


public static class LogStackAnalizator{
    public static EventHandler<LogAnalizator.AnalizeResult> stackEnded =(_,e)=>e.FireLuaEvents();
    private static class deeplinkAnalyzer{
        public static bool isAnalizing = false;
        public static bool isLeaving = false;
        public static bool isJoin = false;
        public static double lastStepTimestamp =0;
        public static void setAnalyze(){
            isAnalizing = true;
        }
        public static void endAnalyzing(){
            isAnalizing = false;
            LogAnalizator.AnalizeResultTypes type =0;
            if (isJoin){
                type = LogAnalizator.AnalizeResultTypes.DeeplinkJoin;
            }else if(isLeaving){
                type = LogAnalizator.AnalizeResultTypes.DeeplinkLeft;
            }
            if (type!=0){
                LogAnalizator.AnalizeResult res = new(type);
                stackEnded.Invoke(type,res);
            }
        }
    }
    public static double Timeout = 1.5;
    public static void checkTimeouts(){
        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() / 1000.0;
        if (deeplinkAnalyzer.isAnalizing){
            if (deeplinkAnalyzer.lastStepTimestamp+Timeout < timestamp){
                deeplinkAnalyzer.endAnalyzing();
            }
        }
    }
    public static LogAnalizator.AnalizeResult? analyze(LogLine line){
        if (line.Sender=="FLog::Output" && line.Data.Contains("App NoReload receiving deeplink")){
            deeplinkAnalyzer.setAnalyze();
            deeplinkAnalyzer.lastStepTimestamp = line.timestamp;
            return new LogAnalizator.AnalizeResult(LogAnalizator.AnalizeResultTypes.NewDeeplinkStack);
        } return null;
    }
    public static class analyzeFromType{
        public static bool analyzeLeave(){
            var res = false;
            if (deeplinkAnalyzer.isAnalizing){
                deeplinkAnalyzer.isLeaving = true;
                res = true;
            }
            return res;
        }
        public static bool analyzeJoin(){
            var res = false;
            if (deeplinkAnalyzer.isAnalizing){
                deeplinkAnalyzer.isJoin = true;
                deeplinkAnalyzer.endAnalyzing();
                res = true;
            }
            return res;
        }
    }
}