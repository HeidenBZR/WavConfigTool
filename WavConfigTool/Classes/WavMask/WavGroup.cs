using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.WavMask
{
    [Serializable]
    public class WavGroup
    {
        public string Name;
        private List<string> wavs = new List<string>();
        private Dictionary<AliasType, AliasTypeMask> aliasTypes = new Dictionary<AliasType, AliasTypeMask>();

        public WavGroup() { }
        public WavGroup(string name, Dictionary<AliasType, AliasTypeMask> aliasTypes, string[] wavFiles)
        {
            Name = name;
            this.aliasTypes = aliasTypes;
            this.wavs = wavFiles.ToList();
        }

        public bool HasWav(string filename)
        {
            return wavs.Contains(filename);
        }

        public bool HasAliasType(AliasType aliasType)
        {
            return aliasTypes.ContainsKey(aliasType);
        }

        public void AddWav(string filename)
        {
            wavs.Add(filename);
        }

        public void AddAliasTypeMask(AliasType aliasType, AliasTypeMask aliasTypeMask = null)
        {
            aliasTypes[aliasType] = aliasTypeMask == null ? new AliasTypeMask() : aliasTypeMask;
        }

        public Dictionary<AliasType, AliasTypeMask> GetAliasTypes() => aliasTypes;

        public List<string> GetWavs() => wavs;
    }
}
