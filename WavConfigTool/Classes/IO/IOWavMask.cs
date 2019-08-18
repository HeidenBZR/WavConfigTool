using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Xml.Serialization;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.IO
{

    public class WavMaskReader
    {
        private static WavMaskReader current;

        public static WavMaskReader GetInstance()
        {
            if (current == null)
            {
                current = new WavMaskReader();
            }
            return current;
        }

        public WavMask Read(string filename)
        {
            var wavMask = ReadYaml(filename);
            return wavMask == null ? new WavMask(false) : wavMask;
        }

        public void Write(string filename, WavMask wavMask)
        {
            if (wavMask == null || wavMask.GetWavGroups() == null)
                return;
            WriteYaml(filename, wavMask);
        }

        private WavMask ReadJson(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new DataContractJsonSerializer(typeof(IOWavMask));
                var ioWavMask = serializer.ReadObject(fileStream) as IOWavMask;
                return ioWavMask == null ? null : GetWavMask(ioWavMask);
            }
        }

        private WavMask ReadXml(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new XmlSerializer(typeof(IOWavMask));
                var ioWavMask = serializer.Deserialize(fileStream) as IOWavMask;
                return ioWavMask == null ? null : GetWavMask(ioWavMask);
            }
        }

        private WavMask ReadYaml(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                var ioWavMask = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOWavMask)) as IOWavMask;
                return ioWavMask == null ? null : GetWavMask(ioWavMask);
            }
        }

        private void WriteJson(string filename, WavMask wavMask)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                var ioWavMask = GetIOWavMask(wavMask);
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IOWavMask));
                serializer.WriteObject(fileStream, ioWavMask);
            }
        }

        private void WriteXml(string filename, WavMask wavMask)
        {
            using (var fileStream = new FileStream(filename, FileMode.Create))
            {
                var ioWavMask = GetIOWavMask(wavMask);
                var serializer = new XmlSerializer(typeof(IOWavMask));
                serializer.Serialize(fileStream, ioWavMask);
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
            var ioWavMask = new IOWavMask();
            ioWavMask.MaxDuplicates = wavMask.MaxDuplicates;
            var list = new List<IOWavGroup>();
            foreach (var wavGroup in wavMask.GetWavGroups())
            {
                var ioWavGroup = new IOWavGroup();
                var iOAliasTypes = new List<IOAliasType>();
                foreach (var pair in wavGroup.GetAliasTypes())
                {
                    var ioAliasType = new IOAliasType();
                    ioAliasType.Name = pair.Key.ToString();
                    ioAliasType.CanTakeFromAllPositions = pair.Value.GetCanTakeAllPositions();
                    if (!ioAliasType.CanTakeFromAllPositions)
                    {
                        ioAliasType.Positions = pair.Value.GetPositions();
                    }
                    iOAliasTypes.Add(ioAliasType);
                }
                ioWavGroup.AliasTypes = iOAliasTypes.ToArray();
                ioWavGroup.WavFiles = wavGroup.GetWavs().ToArray();
                ioWavGroup.Name = wavGroup.Name;
                list.Add(ioWavGroup);
            }
            ioWavMask.WavGroups = list.ToArray();
            return ioWavMask;
        }

        private WavMask GetWavMask(IOWavMask ioWavMask)
        {
            var wavMask = new WavMask();
            wavMask.MaxDuplicates = ioWavMask.MaxDuplicates;
            foreach (var iOWavGroup in ioWavMask.WavGroups)
            {
                var aliasTypes = new Dictionary<AliasType, AliasTypeMask>();
                foreach (IOAliasType iOAliasType in iOWavGroup.AliasTypes)
                {
                    var aliasType = AliasTypeResolver.GetInstance().GetAliasType(iOAliasType.Name);
                    if (aliasType != AliasType.undefined)
                    {
                        aliasTypes[aliasType] = iOAliasType.CanTakeFromAllPositions ? new AliasTypeMask() : new AliasTypeMask(iOAliasType.Positions);
                    }
                }
                var wavGroup = new WavGroup(iOWavGroup.Name, aliasTypes, iOWavGroup.WavFiles);
                wavMask.AddGroup(wavGroup);
            }
            return wavMask;
        }

        [Serializable]
        public class IOWavGroup
        {
            public string Name;
            public string[] WavFiles;
            public IOAliasType[] AliasTypes;
        }

        [Serializable]
        public class IOWavMask
        {
            public IOWavGroup[] WavGroups;
            public int MaxDuplicates;
        }

        [Serializable]
        public class IOAliasType
        {
            public string Name;
            public int[] Positions;
            public bool CanTakeFromAllPositions;
        }
    }
}
