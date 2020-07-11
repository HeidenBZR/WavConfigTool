using System.Collections.Generic;
using System.Linq;

namespace WavConfigCore
{
    public class WavMask
    {
        public int MaxDuplicates;

        internal List<WavGroup> WavGroups { get; private set; }
        internal WavGroup Default { get; private set; }

        public WavMask(bool init = true)
        {
            if (init)
            {
                WavGroups = new List<WavGroup>();
                wavGroupsByFilename = new Dictionary<string, List<WavGroup>>();
            }
        }

        public List<string> GetAliasTypes(string filename)
        {
            var aliasTypes = new List<string>();
            foreach (var wavGroup in wavGroupsByFilename[filename])
            {
                aliasTypes.AddRange(wavGroup.AliasTypes.Select(n => n.ToString()));
            }
            return aliasTypes;
        }

        public bool CanGenerateOnPosition(string filename, AliasType aliasType, int position)
        {
            if (Default == null && (wavGroupsByFilename == null || wavGroupsByFilename.Count == 0))
                return true; // without mask file we generate all possible
            if (wavGroupsByFilename.ContainsKey(filename))
            {
                foreach (var wavGroup in wavGroupsByFilename[filename])
                {
                    if (wavGroup.CanGenerateOnPosition(aliasType, position))
                        return true;
                }
            }
            return Default.CanGenerateOnPosition(aliasType, position);
        }

        public bool CanAddPoint(PhonemeType phonemeType, ProjectLine projectLine)
        {
            if (projectLine.Recline == null)
                return false;
            var phonemes = projectLine.Recline.PhonemesOfType(phonemeType);
            var neededCount = phonemes.Count * 2;
            var realPhonemes = projectLine.PointsOfType(phonemeType, virtuals: false).Count;
            var canIfNoSkip = realPhonemes < neededCount;

            if (!IsConfigured())
                return canIfNoSkip;

            var filename = projectLine.Recline.Name;

            if (!wavGroupsByFilename.ContainsKey(filename))
                return canIfNoSkip;

            var wavGroup = wavGroupsByFilename[filename][0];
            var skips = wavGroup.GetSkippedPhonemesOfType(phonemeType);
            var neededByGroup = 0;
            for(var i = 0; i < phonemes.Count; i++)
            {
                if (!skips.Contains(i))
                    neededByGroup += 2;
            }
            return realPhonemes < neededByGroup;
        }

        public bool MustSkipPhoneme(string filename, PhonemeType phonemeType, int position)
        {
            if (!IsConfigured())
                return false; // without mask file we generate all possible

            if (wavGroupsByFilename.ContainsKey(filename))
            {
                var groups = wavGroupsByFilename[filename];
                foreach (var wavGroup in groups)
                {
                    return wavGroup.MustSkipPhoneme(phonemeType, position);
                }
            }
            return false; // skip only if all groups ask for it
        }

        public void SetDefaultAliasTypes(WavGroup wavGroup)
        {
            Default = wavGroup;
        }

        public void AddGroup(WavGroup wavGroup)
        {
            WavGroups.Add(wavGroup);
            foreach (var wav in wavGroup.Wavs)
            {
                if (!wavGroupsByFilename.ContainsKey(wav))
                {
                    wavGroupsByFilename[wav] = new List<WavGroup>();
                }
                wavGroupsByFilename[wav].Add(wavGroup);
            }
        }

#if DEBUG

        public bool IsInGroup(string filename, string groupName)
        {
            for (var i = 0; i < WavGroups.Count(); i++)
            {
                if (WavGroups[i].Name == groupName)
                {
                    return WavGroups[i].Wavs.Contains(filename);
                }
            }
            return false;
        }
        private bool IsConfigured()
        {
            return Default != null || wavGroupsByFilename != null && wavGroupsByFilename.Count > 0;
        }


#endif

        #region private

        private Dictionary<string, List<WavGroup>> wavGroupsByFilename;

        #endregion
    }
}
