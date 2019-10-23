using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.Reader
{
    class ReplacerReader
    {
        private static ReplacerReader current;
        private ReplacerReader() { }

        public static ReplacerReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new ReplacerReader();
                }
                return current;
            }
        }


    }
}
