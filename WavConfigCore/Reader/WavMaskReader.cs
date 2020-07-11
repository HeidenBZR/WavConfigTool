using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using WavConfigCore.Reader.IO;
using YamlDotNet.Serialization;

namespace WavConfigCore.Reader
{

    public class WavMaskReader
    {
        #region singleton base

        private static WavMaskReader current;
        private WavMaskReader() { }

        public static WavMaskReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new WavMaskReader();
                }
                return current;
            }
        }

        #endregion

        public WavMask Read(string filename)
        {
            var ioWavMask = ReadYaml(filename);
            return ioWavMask != null ? GetWavMask(ioWavMask) : new WavMask(false);
        }

        public void Write(string filename, WavMask wavMask)
        {
            if (wavMask == null || wavMask.WavGroups == null)
                return;
            WriteYaml(filename, GetIOWavMask(wavMask));
        }

        public IOWavMask ReadYaml(string filename)
        {
            IOWavMask ioWavMask = null;
            try
            {
                using (var fileStream = new FileStream(filename, FileMode.Open))
                {
                    var serializer = new Deserializer();
                    ioWavMask = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOWavMask)) as IOWavMask;
                }
            }
            catch { }
            return ioWavMask;
        }

        public void WriteYaml(string filename, IOWavMask ioWavMask)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var serializer = new Serializer();
                serializer.Serialize(writer, ioWavMask, typeof(IOWavMask));
            }
        }

        private IOWavMask GetIOWavMask(WavMask wavMask)
        {
            var ioWavMask = new IOWavMask
            {
                MaxDuplicates = wavMask.MaxDuplicates
            };
            var list = new List<IOWavGroup>();
            foreach (var wavGroup in wavMask.WavGroups)
            {
                var ioWavGroup = new IOWavGroup();
                FillIOWavGroup(ioWavGroup, wavGroup);
                list.Add(ioWavGroup);
            }
            var defaultIOGroup = new IOWavGroup();
            FillIOWavGroup(defaultIOGroup, wavMask.Default);
            ioWavMask.WavGroups = list.ToArray();
            return ioWavMask;
        }

        private WavMask GetWavMask(IOWavMask ioWavMask)
        {
            var wavMask = new WavMask
            {
                MaxDuplicates = ioWavMask.MaxDuplicates
            };
            ioWavMask.Default.WavFiles = new string[0]; // wav files are not needed for default
            var defaultWavGroup = FillWavGroup(ioWavMask.Default);
            wavMask.SetDefaultAliasTypes(defaultWavGroup);

            foreach (var ioWavGroup in ioWavMask.WavGroups)
            {
                var wavGroup = FillWavGroup(ioWavGroup);
                wavMask.AddGroup(wavGroup);
            }
            return wavMask;
        }

        private void FillIOWavGroup(IOWavGroup ioWavGroup, WavGroup wavGroup)
        {
            if (wavGroup == null)
                return;
            var iOAliasTypes = new List<IOAliasType>();
            foreach (var pair in wavGroup.AliasTypes)
            {
                var ioAliasType = new IOAliasType
                {
                    Name = pair.Key.ToString(),
                    CanTakeFromAllPositions = pair.Value.CanTakeAllPositions
                };
                if (!ioAliasType.CanTakeFromAllPositions)
                {
                    ioAliasType.Positions = pair.Value.Positions;
                }
                iOAliasTypes.Add(ioAliasType);
            }
            ioWavGroup.AliasTypes = iOAliasTypes.ToArray();
            ioWavGroup.WavFiles = wavGroup.Wavs.ToArray();
            ioWavGroup.Name = wavGroup.Name;
            ioWavGroup.SkipC = wavGroup.SkipC.ToArray();
            ioWavGroup.SkipV = wavGroup.SkipV.ToArray();
            ioWavGroup.SkipR = wavGroup.SkipR.ToArray();
        }


        private WavGroup FillWavGroup(IOWavGroup ioWavGroup)
        {
            var aliasTypes = new Dictionary<AliasType, AliasTypeMask>();
            foreach (IOAliasType iOAliasType in ioWavGroup.AliasTypes)
            {
                var aliasType = AliasTypeResolver.Current.GetAliasType(iOAliasType.Name);
                if (aliasType != AliasType.undefined)
                {
                    aliasTypes[aliasType] = iOAliasType.CanTakeFromAllPositions ? new AliasTypeMask() : new AliasTypeMask(iOAliasType.Positions);
                }
            }
            var wavGroup = new WavGroup(ioWavGroup.Name, aliasTypes, ioWavGroup.WavFiles);
            wavGroup.SkipC = ioWavGroup.SkipC.ToList();
            wavGroup.SkipR = ioWavGroup.SkipR.ToList();
            wavGroup.SkipV = ioWavGroup.SkipV.ToList();
            return wavGroup;
        }
    }
}
