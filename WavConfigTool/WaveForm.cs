using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;
using NAudio.Wave;

namespace WavConfigTool
{
    class WaveForm
    {
        public static int PointSkip = 5;
        static SolidBrush WavZoneBrush = new SolidBrush(System.Drawing.Color.FromArgb(250, 100, 200, 100));

        public string Path;
        public int SampleRate;
        public List<double> Ds = new List<double>();
        public double MostLeft;

        public long Length;
        public float[] Data;

        public double Threshold = 0.001;
        public double DataThreshold = 0.05;

        public WaveForm(string path)
        {
            Path = path;
            AudioFileReader reader = new AudioFileReader(Path);

            SampleRate = reader.WaveFormat.SampleRate;
            Length = reader.Length;
            Data = new float[Length];
            reader.Read(Data, 0, (int)Length);
            reader.Close();
        }

        double Truncate(double value)
        {
            if (value > 50) return 50;
            else if (value < -50) return -50;
            else if (Math.Abs(value) < Threshold) return 0;
            else return value;
        }

        public System.Windows.Point[] GetAudioPoints()
        {
            List<System.Windows.Point> points = new List<System.Windows.Point>();
            long lastpoint = 0;
            var max = Data.Max();
            long i = 0;
            Length /= 4;
            for (; i < Length; i += PointSkip)
            {
                var x = i * WavControl.ScaleX / SampleRate * 1000;
                var y = Truncate(Data[i] * WavControl.ScaleY * Settings.WAM) + 50;
                points.Add(new System.Windows.Point(x, y));
                if (Data[i] >= max * 0.05)
                {
                    if (Ds.Count == 0)
                    {
                        Ds.Add((double)i / SampleRate * 1000 - 20);
                    }
                    else
                    {
                        if (Ds[0] * WavControl.ScaleX < MostLeft) MostLeft = Ds[0] * WavControl.ScaleX;
                        lastpoint = i;
                    }
                }
            }
            if (Ds.Count < 2) Ds.Add((double)lastpoint / SampleRate * 1000 + 20);
            if (Ds[0] < 0)
                Ds[0] = 40;
            if (Ds[1] > Length - 40)
                Ds[1] = Length - 40;
            return points.ToArray();
        }


        public void PointsToImage(object Data)
        {
            (System.Windows.Point[] points, int w, int h, WavControl control) = ((System.Windows.Point[] points, int w, int h, WavControl control))Data;

            Console.WriteLine($"Started generating {control.ImagePath}");
            Bitmap image = new Bitmap(w, h);
            Graphics waveform = Graphics.FromImage(image);
            waveform.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            System.Drawing.Pen pen = new System.Drawing.Pen(WavZoneBrush);
            PointF[] ps = points.Select(n => new PointF((float)n.X, (float)n.Y)).ToArray();
            waveform.DrawCurve(pen, ps);
            waveform.Save();
            if (!Directory.Exists("Temp")) Directory.CreateDirectory("Temp");
            image.Save(control.ImagePath);
            Console.WriteLine($"Finished {control.ImagePath}");
        }


    }
}
