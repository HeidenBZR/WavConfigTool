using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Tools
{
    static class PathResolver
    {
        public static string Reclist(string filename)
        {
            return Settings.GetResoucesPath(Path.Combine("WavConfigTool", "WavSettings", filename));
        }
    }
}
