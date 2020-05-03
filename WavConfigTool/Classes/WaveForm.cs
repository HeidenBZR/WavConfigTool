using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace WavConfigTool.Classes
{
    public class WaveForm
    {
        public int VisualWidth { get; set; }
        public int Width { get; set; }

        public string Path;

        public WaveFormat WaveFormat;
        public int? RealBitsPerSample => WaveFormat?.BitsPerSample / WaveFormat?.BlockAlign * 2;
        public double MostLeft;

        public double Threshold = 0.001;
        public double DataThreshold = 0.05;

        public bool IsEnabled = false;
        public bool IsGenerating = false;
        public bool IsGenerated = false;

        public Exception GeneratingException;

        public BitmapImage BitmapImage;
        public string ImageHash;

        public WaveForm(string path)
        {
            Path = path;
            IsEnabled = File.Exists(Path);
        }

        public void Start(int height)
        {
            color = Color.FromArgb(255, 100, 200, 100);// "#64c864"
            this.height = height;
            if (!IsEnabled)
            {
                ImageHash = null;
                BitmapImage = null;
                return;
            }
            reader = new AudioFileReader(Path);
        }

        public double GetSampleWidth()
        {
            return Settings.RealToViewX(WaveFormat.Channels * 1000.0 / WaveFormat.SampleRate);
        }

        public void CollectData()
        {
            WaveFormat = reader.WaveFormat;
            // calculate number of samples
            long nSamples = reader.Length / ((WaveFormat.BitsPerSample * WaveFormat.Channels) / 8);
            if (nSamples < 2)
                return;

            int yBase = height / 2;
            double yScale = Settings.UserScaleY;
            double yScaleBase = -((double)height - 3) / 2;
            double sampleWidth = GetSampleWidth();
            double currPosition = 0;
            // Data for current column
            int currColumn = 0;

            float minVal = float.PositiveInfinity, maxVal = float.NegativeInfinity;

            // Data for previous column
            int prevColumn = 0;
            int prevMinY = 0, prevMaxY = 0;

            // Buffer for reading samples
            float[] buffer = new float[8192];
            int readCount;

            points = new List<Point[]>();
            while ((readCount = reader.Read(buffer, 0, 8192)) > 0)
            {
                // process samples
                foreach (float sample in buffer.Take(readCount))
                {
                    minVal = Math.Min(minVal, sample);
                    maxVal = Math.Max(maxVal, sample);
                    currPosition += sampleWidth;

                    // on column change, draw to bitmap
                    if ((int)currPosition >= currColumn)
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
                        currColumn++;
                        minVal = float.PositiveInfinity;
                        maxVal = float.NegativeInfinity;
                    }
                }
            }

            VisualWidth = (int)currPosition;
            Width = Settings.ViewToRealX(VisualWidth);
        }

        public void DrawWaveform()
        {
            var res = new Bitmap(VisualWidth, height);
            using (Brush fillBrush = new SolidBrush(color))
            using (Graphics g = Graphics.FromImage(res))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                foreach (var pointsPair in points)
                {
                    g.FillPolygon(fillBrush, pointsPair, System.Drawing.Drawing2D.FillMode.Winding);
                }
                g.FillRectangle(fillBrush, 0, height / 2, VisualWidth, LINE_WEIGHT);
            }

            BitmapImage = Bitmap2BitmapImage(res);
            res.Dispose();
        }

        public void Finish(string hash)
        {
            points.Clear();
            reader.Close();
            ImageHash = hash;
        }

        private const float LINE_WEIGHT = 0.5f;

        private List<Point[]> points;
        private AudioFileReader reader;
        private int height;
        private Color color;

        private int ClampHeight(int val, int halfHeight)
        {
            return val > halfHeight ? halfHeight : val < -halfHeight ? -halfHeight : val;
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
                bitmapImage.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();

                return bitmapImage;
            }
        }
    }
}
