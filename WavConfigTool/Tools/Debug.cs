using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Tools
{
    static class Debug
    {
        public static void Log(Exception exception, string message = "")
        {
            var text = $"{DateTime.Now.ToString("h:mm:ss tt")}: {message}\n{exception.Message}\n\nCallstack:{exception.StackTrace}\n\n";
            SaveLog(text);
        }

        public static void Log(string message)
        {
            var text = $"{DateTime.Now.ToString("h:mm:ss tt")}: {message}\n\n";
            SaveLog(text);
        }

        static void SaveLog(string text)
        {
            File.WriteAllText(PathResolver.Log(), text, Encoding.UTF8);
        }
    }
}
