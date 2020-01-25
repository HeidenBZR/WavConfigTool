using System;
using System.Collections.Generic;
using System.Linq;

namespace WavConfigCore
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

        internal bool CanGenerateOnPosition(AliasType aliasType, int position)
        {
            return aliasTypes.ContainsKey(aliasType) ? aliasTypes[aliasType].IsAllowedOnPosition(position) : false;
        }
    }
}
