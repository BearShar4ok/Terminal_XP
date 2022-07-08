using System.IO;
using Newtonsoft.Json;

namespace Terminal_XP.Classes
{
    public enum MinimumLevel { Debug, Error, Information }
    
    public static class Logger
    {
        public static MinimumLevel Level { get; set; } = MinimumLevel.Debug;
        
        public static void Debug<T>(T obj) => Debug(JsonConvert.SerializeObject(obj));

        public static void Debug(string log) => System.Diagnostics.Debug.WriteLine(log);

        public static void Error<T>(T obj)
        {
            if (!File.Exists(Addition.ErrorFile))
                File.Create(Addition.ErrorFile);
            
            using (var stream = File.AppendText(Addition.ErrorFile))
            {
                stream.WriteLine(JsonConvert.SerializeObject(obj));
            }
            
            Debug(JsonConvert.SerializeObject(obj));
        }
        
        public static void Error(string log) => Error(log);

        public static void Information<T>(T obj) => Debug(JsonConvert.SerializeObject(obj));
        
        public static void Information(string log) => Debug(log);

        public static void Log(string log, MinimumLevel level = MinimumLevel.Debug)
        {
            {
                switch (level)
                {
                    case MinimumLevel.Debug:
                        Debug(log);
                        break;
                    case MinimumLevel.Information:
                        Information(log);
                        break;
                    case MinimumLevel.Error:
                        Error(log);
                        break;
                    default:
                        Debug(log);
                        break;
                }
            }
        }

        public static void Log<T>(T log, MinimumLevel level = MinimumLevel.Debug)
        {
            switch (level)
            {
                case MinimumLevel.Debug:
                    Debug(log);
                    break;
                case MinimumLevel.Information:
                    Information(log);
                    break;
                case MinimumLevel.Error:
                    Error(log);
                    break;
                default:
                    Debug(log);
                    break;
            }
        }
        
    }
}