using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes.WavMask
{
    [Serializable]
    public class IOWavGroup
    {
        public string[] WavFiles;
        public string[] AliasTypes;
    }

    [Serializable]
    class IOWavMask
    {
        public Dictionary<string, IOWavGroup> WavGroups = new Dictionary<string, IOWavGroup>();
    }

    public class WavMaskReader
    {
        public static WavMaskReader _current;

        public static WavMaskReader GetInstance()
        {
            if (_current == null)
            {
                _current = new WavMaskReader();
            }
            return _current;
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

                foreach (var pair in ioWavMask.WavGroups)
                {
                    var wavGroup = new WavGroup(pair.Key, pair.Value.AliasTypes, pair.Value.WavFiles);
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
                foreach (var wavGroup in wavMask.GetWavGroupsByName())
                {
                    var ioWavGroup = new IOWavGroup();
                    ioWavGroup.AliasTypes = wavGroup.GetAliasTypes().Select(n => n.ToString()).ToArray();
                    ioWavGroup.WavFiles = wavGroup.GetWavs().ToArray();
                    ioWavMask.WavGroups[wavGroup.Name] = ioWavGroup;
                }

                DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(IOWavMask));
                serializer.WriteObject(fileStream, ioWavMask);
            }
        }

    }
}
