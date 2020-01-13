using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
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
        public int VisualWidth;

        public long Length;
        public float[] Data;

        public double Threshold = 0.001;
        public double DataThreshold = 0.05;

        public bool IsEnabled = false;
        public bool IsGenerating = false;
        public bool IsGenerated = false;

        public Exception GeneratingException;

        public System.Windows.Media.ImageSource BitmapImage;
        public string Name = "";

        public WaveForm(string path)
        {
            Path = path + ".wav";
            IsEnabled = File.Exists(Path);
        }

        public void MakeWaveForm(int height, string imagePath, Color color)
        {
            var reader = new AudioFileReader(Path);
            var bitmap = DrawWaveform(reader, height, color);
            BitmapImage = Bitmap2BitmapImage(bitmap);
            Name = imagePath;
        }

        private BitmapImage Bitmap2BitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, System.Drawing.Imaging.ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }

        private const float LINE_WEIGHT = 0.5f;
        // HACK: can't find why it counts wrong, for not i'm just leaving this constant
        public const double X_SCALE_ERROR = 1.379;

        private Bitmap DrawWaveform(AudioFileReader reader, int height, Color color)
        {
            // calculate number of samples
            long nSamples = reader.Length / ((reader.WaveFormat.BitsPerSample * reader.WaveFormat.Channels) / 8);
            if (nSamples < 2)
                return null;
            
            int yBase = height / 2;
            double yScale = Project.Current.WavAmplitudeMultiplayer;
            double yScaleBase = -((double)height - 3) / 2;

            double sampleWidth = Settings.RealToViewX(1.0 / reader.WaveFormat.BitsPerSample * reader.WaveFormat.Channels / X_SCALE_ERROR);
            double currPosition = 0;
            // Data for current column
            int currColumn = 0;
            var points = new List<Point[]>();
            Bitmap res;
            using (Pen linePen = new Pen(Color.Red))
            using (Brush fillBrush = new SolidBrush(color))
            {
                //g.Clear(Color.Black);

                float minVal = float.PositiveInfinity, maxVal = float.NegativeInfinity;

                // Data for previous column
                int prevColumn = 0;
                int prevMinY = 0, prevMaxY = 0;

                // Buffer for reading samples
                float[] buffer = new float[8192];
                int readCount;

                while ((readCount = reader.Read(buffer, 0, 8192)) > 0)
                {

                    // process samples
                    foreach (float sample in buffer.Take(readCount))
                    {
                        minVal = Math.Min(minVal, sample);
                        maxVal = Math.Max(maxVal, sample);
                        currPosition += sampleWidth;

                        // on column change, draw to bitmap
                        if ((int)currPosition > currColumn)
                        {
                            if (!float.IsInfinity(minVal) && !float.IsInfinity(maxVal))
                            {
                                // calculate Y coordinates for min & max
                                int minY = ClampHeight((int)(yScaleBase * yScale * minVal), (int)height / 2);
                                int maxY = ClampHeight((int)(yScaleBase * yScale * maxVal), (int)height / 2);

                                points.Add(new Point[] {
                                        new Point(prevColumn, yBase + prevMinY), new Point(prevColumn, yBase + prevMaxY),
                                        new Point(currColumn, yBase + maxY), new Point(currColumn, yBase + minY)
                                });

                                // save current data to previous
                                prevColumn = currColumn;
                                prevMinY = minY;
                                prevMaxY = maxY;
                            }

                            // update column number and reset accumulators
                            currColumn = (int)currPosition;
                            minVal = float.PositiveInfinity;
                            maxVal = float.NegativeInfinity;
                        }
                    }
                }

                VisualWidth = (int)currPosition;
                res = new Bitmap(VisualWidth, height);

                using (Graphics g = Graphics.FromImage(res))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                    foreach (var pointsPair in points)
                    {
                        g.FillPolygon(fillBrush, pointsPair, System.Drawing.Drawing2D.FillMode.Winding);
                    }
                    g.FillRectangle(fillBrush, 0, height / 2, VisualWidth, LINE_WEIGHT);
                }
            }

            return res;
        }

        private int ClampHeight(int val, int halfHeight)
        {
            return val > halfHeight ? halfHeight : val < -halfHeight ? -halfHeight : val; 
        }

    }
}
