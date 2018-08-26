using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Media;
using System.IO;
using System.Windows.Media.Imaging;

namespace WavConfigTool
{
    class WaveForm
    {
        static SolidBrush WavZoneBrush = new SolidBrush(System.Drawing.Color.FromArgb(250, 100, 200, 100));

        public static void PointsToImage(object data)
        {
            (System.Windows.Point[] points, int w, int h, WavControl control) = ((System.Windows.Point[] points, int w, int h, WavControl control))data;

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
