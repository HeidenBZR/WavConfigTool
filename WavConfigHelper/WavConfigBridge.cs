using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore;
using WavConfigCore.Tools;
using WavConfigCore.Reader;

namespace WavConfigHelper
{
    class WavConfigBridge
    {
        public WavConfigBridge()
        {
            foreach (string filename in Directory.GetFiles(PathResolver.Current.Reclist(), "*.reclist"))
            {
                var reclist = Path.GetFileNameWithoutExtension(filename);
                if (!reclists.Contains(reclist))
                    reclists.Add(reclist);
            }
        }

        public bool HasReclist(string name)
        {
            return reclists.Contains(name);
        }

        public bool ImportProject(string inputFilename, string reclistName, string outputFilename)
        {
            try
            {
                var project = WavconfigReader.Current.Read(inputFilename, reclistName);
                ProjectReader.Current.Write(outputFilename, project);
            }
            catch (Exception ex)
            {
                Catch(ex);
                return false;
            }
            return true;
        }

        public bool ImportReclist(string inputFilename, string reclistPath)
        {
            try
            {
                var reclist = WavSettingsReader.Current.Read(inputFilename);
                ReclistReader.Current.Write(reclistPath, reclist);
            }
            catch (Exception ex)
            {
                Catch(ex);
                return false;
            }
            return true;
        }

        #region private

        private readonly List<string> reclists = new List<string>();

        private void Catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        #endregion
    }
}
