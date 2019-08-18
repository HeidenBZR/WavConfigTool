using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.Mask
{
    [Serializable]
    public class WavMask
    {
        private Dictionary<string, List<WavGroup>> wavGroupsByFilename;
        private List<WavGroup> wavGroups;

        public WavMask()
        {
            wavGroups = new List<WavGroup>();
            wavGroupsByFilename = new Dictionary<string, List<WavGroup>>();
        }

        public List<string> GetAliasTypes(string filename)
        {
            var aliasTypes = new List<string>();
            foreach (var wavGroup in wavGroupsByFilename[filename])
            {
                aliasTypes.AddRange(wavGroup.GetAliasTypes().Select(n => n.ToString()));
            }
            return aliasTypes;
        }

        public void AddGroup(WavGroup wavGroup)
        {
            wavGroups.Add(wavGroup);
            foreach (var wav in wavGroup.GetWavs())
            {
                if (!wavGroupsByFilename.ContainsKey(wav))
                {
                    wavGroupsByFilename[wav] = new List<WavGroup>();
                }
                wavGroupsByFilename[wav].Add(wavGroup);
            }
        }

        public List<WavGroup> GetWavGroupsByName()
        {
            return wavGroups;
        }
    }
}
