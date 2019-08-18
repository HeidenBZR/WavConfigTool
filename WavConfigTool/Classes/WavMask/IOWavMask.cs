using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.WavMask
{

    public class WavMaskReader
    {
        public static WavMaskReader current;

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
            var wavMask = new WavMask();
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IOWavMask));
                var ioWavMask = serializer.ReadObject(fileStream) as IOWavMask;
                if (ioWavMask == null)
                    return null;

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
            }
            return wavMask;
        }

        public void Write(string filename, WavMask wavMask)
        {
            if (wavMask == null || wavMask.GetWavGroupsByName() == null)
                return;
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var ioWavMask = new IOWavMask();
                var list = new List<IOWavGroup>();
                foreach (var wavGroup in wavMask.GetWavGroupsByName())
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

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IOWavMask));
                serializer.WriteObject(fileStream, ioWavMask);
            }
        }

        [Serializable]
        public class IOWavGroup
        {
            public string Name;
            public string[] WavFiles;
            public IOAliasType[] AliasTypes;
        }

        [Serializable]
        class IOWavMask
        {
            public IOWavGroup[] WavGroups;
        }

        [Serializable]
        public class IOAliasType
        {
            public string Name;
            public int[] Positions;
            public bool CanTakeFromAllPositions = true;
        }
    }
}
