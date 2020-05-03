using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.Classes
{
    public class Frq
    {
        public double[] Points { get; set; }
        public string Name;
        public double AverageFrequency;
        public int SamplesPerFrq;

        public string Title;

        public void Load(string wavFilename)
        {
            if (!wavFilename.Contains(".wav"))
            {
                Reset();
                return;
            }
            Name = wavFilename.Replace(".wav", "_wav.frq");
            try
            {
                Read();
            }
            catch
            {
                Reset();
            }
        }

        public void Reset()
        {
            Points = null;
            Name = null;
        }

        #region private

        private void Read()
        {
            byte[] bytes;

            List<double> frequency = new List<double>();
            List<double> amplitude = new List<double>();
            using (var s = new FileStream(Name, FileMode.Open))
            {
                bytes = new byte[24];
                s.Read(bytes, offset: 0, count: 24);
                Title = Encoding.ASCII.GetString(bytes, index: 0, count: 8);
                SamplesPerFrq = BitConverter.ToInt32(bytes, startIndex: 8);
                AverageFrequency = BitConverter.ToDouble(bytes, startIndex: 12);
                for (int offset = 0; offset + 16 < s.Length; offset += 16)
                {
                    bytes = new byte[16];
                    s.Read(bytes, 0, 16);
                    frequency.Add(BitConverter.ToDouble(bytes, startIndex: 0));
                    amplitude.Add(BitConverter.ToDouble(bytes, startIndex: 8));
                }
            }
            Points = frequency.ToArray();
        }

        #endregion
    }
}
