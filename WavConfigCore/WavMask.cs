﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace WavConfigCore
{
    [Serializable]
    public class WavMask
    {
        private Dictionary<string, List<WavGroup>> wavGroupsByFilename;
        private List<WavGroup> wavGroups;
        public int MaxDuplicates;

        public WavMask(bool init = true)
        {
            if (init)
            {
                wavGroups = new List<WavGroup>();
                wavGroupsByFilename = new Dictionary<string, List<WavGroup>>();
            }
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

        public bool CanGenerateOnPosition(string filename, AliasType aliasType, int position)
        {
            if (wavGroupsByFilename == null || wavGroupsByFilename.Count == 0)
                return true; // without mask file we generate all possible
            if (wavGroupsByFilename.ContainsKey(filename))
            {
                foreach (var wavGroup in wavGroupsByFilename[filename])
                {
                    if (wavGroup.CanGenerateOnPosition(aliasType, position))
                        return true;
                }
            }
            return false;
        }

        public List<WavGroup> GetWavGroups() => wavGroups;
    }
}
