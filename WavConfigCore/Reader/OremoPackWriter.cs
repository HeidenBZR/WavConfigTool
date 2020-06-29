using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Tools;

namespace WavConfigCore.Reader
{
    public class OremoPackWriter
    {
        #region singleton base

        private static OremoPackWriter current;
        private OremoPackWriter() { }

        public static OremoPackWriter Current
        {
            get
            {
                if (current == null)
                {
                    current = new OremoPackWriter();
                }
                return current;
            }
        }

        #endregion
    
        public void WriteAndOpenFolder(OremoPack oremoPack)
        {
            var dir = PathResolver.Current.OremoPack(oremoPack.Name);
            if (!Directory.Exists(dir))
                return;
            var reclistDir = Path.Combine(dir, "reclist.txt");
            var commentDir = Path.Combine(dir, "oremo-comments.txt");
            try
            {
                File.WriteAllText(reclistDir, oremoPack.Reclist, Encoding.GetEncoding(932));
                File.WriteAllText(commentDir, oremoPack.Comment, Encoding.GetEncoding(932));
            }
            catch (Exception)
            {

            }
            
            Process.Start(new ProcessStartInfo()
            {
                FileName = dir,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
