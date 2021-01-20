using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using Brush = System.Drawing.Brush;
using Color = System.Drawing.Color;
using Point = System.Windows.Point;

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

        public Point[] CalculateVisualPoints(WaveForm waveForm, double height)
        {
            var middleHeight = height / 2;

            double maxVal = 0;
            var averagedPoints = new List<double>();
            foreach (var point in Points)
            {
                var average = point - AverageFrequency;
                averagedPoints.Add(average);
                if (average != 0 && maxVal < Math.Abs(average))
                    maxVal = Math.Abs(average);
            }

            var step = Settings.RealToViewX(waveForm.Channels * 1000.0 * SamplesPerFrq /
                                            waveForm.SampleRate);

            var x = step / 2;
            var points = new List<Point>();
            for (var i = 1; i < averagedPoints.Count; i++)
            {
                var point = averagedPoints[i];
                var saturatedY = point / maxVal;
                var y = middleHeight - saturatedY * middleHeight;
                if (y != height)
                {
                    if (y < 0 || y > height)
                        throw new Exception("error on draw frq");

                    points.Add(new Point(x, y));
                }

                x += step;
            }

            return points.ToArray();
        }

        public Bitmap DrawPoints(Point[] points, float height)
        {
            var width = (int) (points[points.Length - 1].X + 1);
            var res = new Bitmap(width, (int)height);
            using (var fillBrush = new SolidBrush(color))
            using (var pen = new Pen(color))
            using (Graphics g = Graphics.FromImage(res))
            {
                var path = new System.Drawing.Drawing2D.GraphicsPath();
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                for (int i = 0; i + 1 < points.Length; i++)
                {
                    Point p = points[i];
                    Point p2 = points[i + 1];
                    g.DrawLine(pen, (float)p.X, (float)p.Y, (float)p2.X, (float)p2.Y);
                    g.FillRectangle(fillBrush, (float)p.X - SIZE / 2f, (float)p.Y - SIZE / 2f, SIZE, SIZE);
                }
            }

            return res;
        }

        #region private

        private readonly Color color = Color.FromArgb(248, 36, 36);
        private const int SIZE = 4;

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
