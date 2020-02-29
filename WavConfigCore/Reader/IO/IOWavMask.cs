using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigCore.Reader.IO
{

    [Serializable]
    public class IOAliasType
    {
        public string Name;
        public int[] Positions = new int[0];
        public bool CanTakeFromAllPositions;
    }

    [Serializable]
    public class IOWavGroup
    {
        public string Name;
        public string[] WavFiles = new string[0];
        public IOAliasType[] AliasTypes = new IOAliasType[0];
    }

    [Serializable]
    public class IOWavMask
    {
        public IOWavGroup[] WavGroups = new IOWavGroup[0];
        public IOWavGroup Default = new IOWavGroup();
        public int MaxDuplicates;
    }
}
