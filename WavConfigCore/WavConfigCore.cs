using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Reader;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class WavConfigCore
    {
        #region singleton base

        private static WavConfigCore current;
        private WavConfigCore() { }

        public static WavConfigCore Current
        {
            get
            {
                if (current == null)
                {
                    current = new WavConfigCore();
                }
                return current;
            }
        }

        #endregion

        public Reclist[] GetReclists()
        {
            var reclists = new List<Reclist>();
            var list = new List<string>();
            string path = PathResolver.Current.Reclist();
            var files = Directory.GetFiles(path, "*.reclist");
            foreach (string filename in files)
            {
                var reclist = ReadReclist(filename, list);
                if (reclist != null)
                    reclists.Add(reclist);
            }
            var testPath = Path.Combine(path, PathResolver.TEST_FOLDER);
            if (Directory.Exists(testPath))
            {
                var filesTest = Directory.GetFiles(testPath, "*.reclist");
                foreach (string filename in filesTest)
                {
                    var reclist = ReadReclist(filename, list, true);
                    if (reclist != null)
                        reclists.Add(reclist);
                }
            }
            return reclists.ToArray();
        }

        #region private
        private Reclist ReadReclist(string filename, List<string> list, bool isTest = false)
        {
            var reclistName = Path.GetFileNameWithoutExtension(filename);
            var reclist = isTest ? ReclistReader.Current.ReadTest(reclistName) : ReclistReader.Current.Read(reclistName);
            if (reclist != null && reclist.IsLoaded && !list.Contains(reclistName))
            {
                list.Add(reclistName);
                return reclist;
            }
            return null;
        }

        #endregion
    }
}
