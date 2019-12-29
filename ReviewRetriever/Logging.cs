using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace ReviewRetriever
{
    enum LogLevel 
    {
        Console = 0,
        LogFile = 1,
        Both = 2
    }

    static class Logging
    {
        public static void Log(string text, LogLevel logLevel) 
        {
            string sLogPath = config.LogPath + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
            PrepareFile(sLogPath);
            
            if(logLevel == LogLevel.LogFile || logLevel == LogLevel.Both)
                File.AppendAllText(sLogPath, "[ " + DateTime.Now.ToString() + "] - " + text + "\n");
            
            if (logLevel == LogLevel.Console|| logLevel == LogLevel.Both)
                Console.WriteLine(text);
        }


        private static void PrepareFile(string sLogPath) 
        {
            if (!Directory.Exists(Path.GetDirectoryName(sLogPath)))
                Directory.CreateDirectory(Path.GetDirectoryName(sLogPath));
            if (!File.Exists(sLogPath))
                File.Create(sLogPath).Close();
        }
    }
}
