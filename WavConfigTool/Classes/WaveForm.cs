using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using NAudio.Wave;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class WaveForm
    {
        public static int PointSkip = 5;
        public const string WAV_ZONE_COLOR = "#64c864";
        //public static SolidBrush WavZoneBrush = new SolidBrush(System.Drawing.Color.FromArgb(250, 100, 200, 100));
        //public static System.Drawing.Pen PEN = new System.Drawing.Pen(WavZoneBrush);

        public string Path;
        public int SampleRate;
        public int BitRate;
        //public List<double> Ds = new List<double>();
        public double MostLeft;

        public long Length;
        public float[] Data;

        public double Threshold = 0.001;
        public double DataThreshold = 0.05;

        public bool IsEnabled = false;
        public bool IsGenerating = false;
        public bool IsGenerated = false;
        
        public Exception GeneratingException;

        public WaveForm(string path)
        {
            Path = path + ".wav";
            Init();
        }

        public void Init()
        {
            if (!File.Exists(Path))
                return;
            else
                IsEnabled = true;

            AudioFileReader reader = new AudioFileReader(Path);

            SampleRate = reader.WaveFormat.SampleRate;
            BitRate = reader.WaveFormat.BitsPerSample;
            Length = reader.Length / 4;
            Data = new float[Length];
            reader.Read(Data, 0, (int)Length);
            reader.Close();

        }

        float Truncate(float value)
        {
            if (value > 50) return 50;
            else if (value < -50) return -50;
            else if (Math.Abs(value) < Threshold) return 0;
            else return value;
        }

        public PointF[] GetAudioPoints()
        {
            try
            {
                if (Data is null)
                    Init();
                var points = new List<PointF>();
                var max = Data.Max();
                long i = 0;
                float factorX = (float)(Settings.RealToViewX(1000 / (double)SampleRate));
                float factorY = (float)(Settings.RealToViewY(Settings.WAM));

                float prev = 0;
                float preprev = 0;
                for (; i < Length; i += PointSkip)
                {
                    float x = i * factorX;
                    float y = Truncate(Data[i] * factorY) + 50;
                    if (i > 0 || y != prev && y != preprev)
                        points.Add(new PointF(x, y));
                    prev = y;
                    preprev = prev;
                }
                Data = null;
                return points.ToArray();
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        //public System.Windows.Point[] GetAudioPoints()
        //{
        //    List<System.Windows.Point> points = new List<System.Windows.Point>();
        //    long lastpoint = 0;
        //    var max = Data.Max();
        //    long i = 0;
        //    Length /= 4;
        //    for (; i < Length; i += PointSkip)
        //    {
        //        var x = i * WavControl.ScaleX / SampleRate * 1000;
        //        var y = Truncate(Data[i] * WavControl.ScaleY * Settings.WAM) + 50;
        //        points.Add(new System.Windows.Point(x, y));
        //        if (Data[i] >= max * 0.05)
        //        {
        //            if (Ds.Count == 0)
        //            {
        //                Ds.Add((double)i / SampleRate * 1000 - 20);
        //            }
        //            else
        //            {
        //                if (Ds[0] * WavControl.ScaleX < MostLeft) MostLeft = Ds[0] * WavControl.ScaleX;
        //                lastpoint = i;
        //            }
        //        }
        //    }
        //    if (Ds.Count < 2) Ds.Add((double)lastpoint / SampleRate * 1000 + 20);
        //    if (Ds[0] < 0)
        //        Ds[0] = 40;
        //    if (Ds[1] > Length - 40)
        //        Ds[1] = Length - 40;
        //    return points.ToArray();
        //}

        public void PointsToImage(PointF[] points, int w, int h, string imagePath, System.Drawing.Pen pen)
        {
            try
            {
                IsGenerated = false;
                Bitmap image = new Bitmap(w, h);
                Graphics waveform = Graphics.FromImage(image);
                waveform.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                waveform.DrawCurve(pen, points);
                waveform.Save();
                image.Save(imagePath);
                waveform.Dispose();
                image.Dispose();
                IsGenerating = false;
                IsGenerated = true;
            }
            catch (Exception ex)
            {
                IsGenerating = false;
                IsGenerated = false;
                GeneratingException = ex;
            }
        }

        public void PointsToImage(object data)
        {
            try
            {
                IsGenerated = false;
                (PointF[] points, int w, int h, string imagePath, System.Drawing.Pen pen ) = 
                    ((PointF[] points, int w, int h, string imagePath, System.Drawing.Pen))data;

                Bitmap image = new Bitmap(w, h);
                Graphics waveform = Graphics.FromImage(image);
                waveform.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                waveform.DrawCurve(pen, points);
                waveform.Save();
                image.Save(imagePath);
                waveform.Dispose();
                image.Dispose();
                IsGenerating = false;
                IsGenerated = true;
            }
            catch (Exception ex)
            {
                IsGenerating = false;
                IsGenerated = false;
                GeneratingException = ex;
            }
        }


    }
}
