using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.Reader.IO
{

    [Serializable]
    public class IOAliasType
    {
        public string Name;
        public int[] Positions;
        public bool CanTakeFromAllPositions;
    }

    [Serializable]
    public class IOWavGroup
    {
        public string Name;
        public string[] WavFiles;
        public IOAliasType[] AliasTypes;
    }

    [Serializable]
    public class IOWavMask
    {
        public IOWavGroup[] WavGroups;
        public int MaxDuplicates;
    }
}
