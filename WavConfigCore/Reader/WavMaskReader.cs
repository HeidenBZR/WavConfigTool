using System;
using System.Collections.Generic;
using System.IO;
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
            var wavMask = ReadYaml(filename);
            return wavMask ?? new WavMask(false);
        }

        public void Write(string filename, WavMask wavMask)
        {
            if (wavMask == null || wavMask.WavGroups == null)
                return;
            WriteYaml(filename, wavMask);
        }

        private WavMask ReadYaml(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                IOWavMask ioWavMask = null;
                try
                {
                    ioWavMask = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOWavMask)) as IOWavMask;
                }
                catch { }
                return ioWavMask == null ? null : GetWavMask(ioWavMask);
            }
        }

        private void WriteYaml(string filename, WavMask wavMask)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var ioWavMask = GetIOWavMask(wavMask);
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
                list.Add(ioWavGroup);
            }
            ioWavMask.WavGroups = list.ToArray();
            return ioWavMask;
        }

        private WavMask GetWavMask(IOWavMask ioWavMask)
        {
            var wavMask = new WavMask
            {
                MaxDuplicates = ioWavMask.MaxDuplicates
            };

            var aliasTypes = new Dictionary<AliasType, AliasTypeMask>();
            foreach (IOAliasType iOAliasType in ioWavMask.Default.AliasTypes)
            {
                var aliasType = AliasTypeResolver.Current.GetAliasType(iOAliasType.Name);
                if (aliasType != AliasType.undefined)
                {
                    aliasTypes[aliasType] = iOAliasType.CanTakeFromAllPositions ? new AliasTypeMask() : new AliasTypeMask(iOAliasType.Positions);
                }
            }
            var wavGroup = new WavGroup(aliasTypes);
            wavMask.SetDefaultAliasTypes(wavGroup);

            foreach (var iOWavGroup in ioWavMask.WavGroups)
            {
                aliasTypes = new Dictionary<AliasType, AliasTypeMask>();
                foreach (IOAliasType iOAliasType in iOWavGroup.AliasTypes)
                {
                    var aliasType = AliasTypeResolver.Current.GetAliasType(iOAliasType.Name);
                    if (aliasType != AliasType.undefined)
                    {
                        aliasTypes[aliasType] = iOAliasType.CanTakeFromAllPositions ? new AliasTypeMask() : new AliasTypeMask(iOAliasType.Positions);
                    }
                }
                wavGroup = new WavGroup(iOWavGroup.Name, aliasTypes, iOWavGroup.WavFiles);
                wavMask.AddGroup(wavGroup);
            }
            return wavMask;
        }
    }
}
