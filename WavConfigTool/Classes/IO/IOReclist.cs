using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.IO
{
    class WavSettingsReader
    {
        private static WavSettingsReader current;
        private WavSettingsReader() { }

        public static WavSettingsReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new WavSettingsReader();
                }
                return current;
            }
        }
    }
    class ReclistReader
    {
        private static ReclistReader current;
        private ReclistReader() { }

        public static ReclistReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new ReclistReader();
                }
                return current;
            }
        }

    }

    class IOReclist
    {
    }
}
