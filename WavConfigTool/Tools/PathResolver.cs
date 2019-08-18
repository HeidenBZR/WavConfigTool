using System.IO;

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
