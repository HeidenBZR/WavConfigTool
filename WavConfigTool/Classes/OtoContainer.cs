using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore;

namespace WavConfigTool.Classes
{
    class OtoContainer : PagerContentBase
    {
        public Oto Oto { get; private set; }
        public ProjectLineContainer BaseProjectLineContainer { get; private set; }

        public OtoContainer(Oto oto, ProjectLineContainer baseContainer)
        {
            Oto = oto;
            BaseProjectLineContainer = baseContainer;
        }
    }
}
