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
        private List<AliasType> aliasTypes = new List<AliasType>();

        public WavGroup() { }
        public WavGroup(string name, string[] aliasTypes, string[] wavFiles)
        {
            Name = name;
            this.aliasTypes = new List<AliasType>();
            foreach (var aliasTypeString in aliasTypes)
            {
                var aliasType = AliasTypeResolver.GetInstance().GetAliasType(aliasTypeString);
                if (aliasType != AliasType.undefined  && !this.aliasTypes.Contains(aliasType))
                    this.aliasTypes.Add(aliasType);
            }
            this.wavs = wavFiles.ToList();
        }

        public bool HasWav(string filename)
        {
            return wavs.Contains(filename);
        }

        public bool HasAliasType(AliasType aliasType)
        {
            return aliasTypes.Contains(aliasType);
        }

        public void AddWav(string filename)
        {
            wavs.Add(filename);
        }

        public void AddAliasType(AliasType aliasType)
        {
            aliasTypes.Add(aliasType);
        }

        public List<AliasType> GetAliasTypes()
        {
            return aliasTypes;
        }

        public List<string> GetWavs()
        {
            return wavs;
        }
    }
}
