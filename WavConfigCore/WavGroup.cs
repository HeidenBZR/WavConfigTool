using System;
using System.Collections.Generic;
using System.Linq;

namespace WavConfigCore
{
    [Serializable]
    public class WavGroup
    {
        public string Name;

        internal List<string> Wavs { get; private set; }
        internal Dictionary<AliasType, AliasTypeMask> AliasTypes { get; private set; }

        public WavGroup() 
        {
            Wavs = new List<string>();
            AliasTypes = new Dictionary<AliasType, AliasTypeMask>();
        }

        public WavGroup(Dictionary<AliasType, AliasTypeMask> aliasTypes) : this()
        {
            AliasTypes = aliasTypes;
        }

        public WavGroup(string name, Dictionary<AliasType, AliasTypeMask> aliasTypes, string[] wavFiles) : this(aliasTypes)
        {
            Name = name;
            AliasTypes = aliasTypes;
            Wavs = wavFiles.ToList();
        }

        public bool HasWav(string filename)
        {
            return Wavs.Contains(filename);
        }

        public bool CanGenerateOnPosition(AliasType aliasType, int position)
        {
            return AliasTypes.ContainsKey(aliasType) ? AliasTypes[aliasType].IsAllowedOnPosition(position) : false;
        }

        public void AddWav(string filename)
        {
            Wavs.Add(filename);
        }

        public void AddAliasTypeMask(AliasType aliasType, AliasTypeMask aliasTypeMask = null)
        {
            AliasTypes[aliasType] = aliasTypeMask ?? new AliasTypeMask();
        }

    }
}
