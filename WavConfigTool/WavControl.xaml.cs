using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NAudio;
using NAudio.Wave;

namespace WavConfigTool
{
    public enum WavConfigPoint
    {
        D,
        V,
        Cf,
        Co
    }
    /// <summary>
    /// Логика взаимодействия для WavControl.xaml
    /// </summary>
    public partial class WavControl : UserControl
    {
        public Recline Recline;
        public List<double> Cos;
        public List<double> Cfs;
        public List<double> Vs;
        public List<double> Ds;

        public WavMarker[] Data = new WavMarker[2]; 

        public static double ScaleX = 700f;
        public static int SampleRate = 44100;
        public static double ScaleY = 100f;
        public static int PointSkip = 5;
        public static double MostLeft = 999999;

        SolidColorBrush CutZoneBrush = new SolidColorBrush(Color.FromArgb(90, 200, 100, 100));
        SolidColorBrush VowelZoneBrush = new SolidColorBrush(Color.FromArgb(250, 200, 50, 200));
        SolidColorBrush CfZoneBrush = new SolidColorBrush(Color.FromArgb(250, 200, 200, 50));
        SolidColorBrush WavZoneBrush = new SolidColorBrush(Color.FromArgb(250, 100, 200, 100));

        public WavControl(Recline recline)
        {
            Recline = recline;
            InitializeComponent();
            Cos = new List<double>();
            Cfs = new List<double>();
            Vs = new List<double>();
            Ds = new List<double>();
            LabelName.Content = recline.Description;
            Draw();
        }


        override protected void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            double x = e.GetPosition(this).X;
            if (Keyboard.IsKeyDown(Key.V))
                Draw(WavConfigPoint.V, x);
            else if (Keyboard.IsKeyDown(Key.C) || Keyboard.IsKeyDown(Key.O))
                Draw(WavConfigPoint.Co, x);
            else if (Keyboard.IsKeyDown(Key.F))
                Draw(WavConfigPoint.Cf, x);
            else if (Keyboard.IsKeyDown(Key.D))
                Draw(WavConfigPoint.D, x);
        }

        public void Draw()
        {
            AudioFileReader reader = new AudioFileReader(Recline.Filename);
            SampleRate = reader.WaveFormat.SampleRate;
            var l = reader.Length;
            float[] data = new float[l];
            reader.Read(data, 0, (int)l);
            Polyline line = new Polyline();
            long lastpoint = 0;
            var max = data.Max();
            for (long i = 0; i < l; i += PointSkip)
            {
                if (Math.Abs(data[i]) > 0.001)
                    line.Points.Add(new Point(i * ScaleX / SampleRate, data[i] * ScaleY + 50));
                else
                {
                    line.Points.Add(new Point(i * ScaleX / SampleRate, 50));
                }
                if (data[i] >= max * 0.1)
                {
                    if (Ds.Count == 0)
                    {
                        Ds.Add((double)i / SampleRate);
                        if (Ds[0] * ScaleX < MostLeft) MostLeft = Ds[0] * ScaleX;
                    }
                        
                    else
                        lastpoint = i;
                }
            }
            Ds.Add((double)lastpoint / SampleRate);
            line.SnapsToDevicePixels = true;
            line.Stroke = WavZoneBrush;
            WavCanvas.Children.Add(line);
            Height = 100;
            Width = line.Points.Last().X;
            Background = new SolidColorBrush(Color.FromArgb(20, 90, 200, 200));
            Margin = new Thickness(0, 10, 0, 10);
            HorizontalAlignment = HorizontalAlignment.Left;
            DrawConfig();
        }


        void Draw(WavConfigPoint point, double x)
        {
            switch (point)
            {
                case WavConfigPoint.Co:
                    Cos.Add(x / ScaleX);
                    break;
                case WavConfigPoint.Cf:
                    Cfs.Add(x / ScaleX);
                    break;
                case WavConfigPoint.V:
                    Vs.Add(x / ScaleX);
                    break;
                case WavConfigPoint.D:
                    if (Ds.Count < 2) Ds.Add(x / ScaleX);
                    else
                    {
                        var distance1 = Math.Abs(Ds[0] - x);
                        var distance2 = Math.Abs(Ds[1] - x);
                        if (distance1 < distance2) distance1 = x;
                        else  distance2 = x;
                    }
                    break;
            }
            DrawConfig();
        }

        void DrawConfig()
        {
            Ds.Sort();
            Vs.Sort();
            Cos.Sort();
            Cfs.Sort();
            AreasCanvas.Children.Clear();
            MarkerCanvas.Children.Clear();
            DrawD();
            DrawV();
            DrawCo();
            DrawCf();
        }

        void DrawV()
        {
            for (int i = 0; i < Vs.Count; i++)
            {
                WavMarker line;
                double pos = Vs[i];
                if (i / 2 < Recline.Vowels.Count)
                    line = new WavMarker((Vowel)Recline.Vowels[i / 2], pos, i % 2);
                else line = new WavMarker(new Vowel("{V}"), pos, i % 2);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Vs.Remove(pos);
                    DrawConfig();
                };
            }
            for (int i = 0; i + 1 < Vs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = VowelZoneBrush,
                    Points = new PointCollection
                    {
                        new Point((Vs[i]) * ScaleX, 50),
                        new Point((Vs[i] + 0.1) * ScaleX, 30),
                        new Point((Vs[i + 1] - 0.1) * ScaleX, 30),
                        new Point((Vs[i + 1]) * ScaleX, 50),
                        new Point((Vs[i + 1] - 0.1) * ScaleX, 70),
                        new Point((Vs[i] + 0.1) * ScaleX, 70)
                    }
                };
                AreasCanvas.Children.Add(Zone);
            }
        }
        void DrawD()
        {
            for (int i = 0; i < Ds.Count; i++)
            {
                double pos = Ds[i];
                WavMarker line = new WavMarker(pos);
                Data[i] = line;
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Ds.Remove(pos);
                    DrawConfig();
                };
            }
            if (Ds.Count == 0) return;
            var x = Ds[0] * ScaleX;
            Polygon ZoneIn = new Polygon()
            {
                Fill = CutZoneBrush,
                Points = new PointCollection()
                {
                    new Point(0,0),
                    new Point(x,0),
                    new Point(x,0),
                    new Point(x - 0.1 * ScaleX,50),
                    new Point(x,100),
                    new Point(0,100)
                }
            };
            AreasCanvas.Children.Add(ZoneIn);
            if (Ds.Count == 1) return;
            x = Ds[1] * ScaleX;
            Polygon ZoneOut = new Polygon()
            {
                Fill = CutZoneBrush,
                Points = new PointCollection()
                {
                    new Point(Width,0),
                    new Point(x,0),
                    new Point(x,0),
                    new Point(x + 0.1 * ScaleX,50),
                    new Point(x,100),
                    new Point(Width,100)
                }
            };
            AreasCanvas.Children.Add(ZoneOut);
        }
        void DrawCo()
        {
            for (int i = 0; i < Cos.Count; i++)
            {
                double pos = Cos[i];
                WavMarker line;
                if (i / 2 < Recline.Occlusives.Count)
                    line = new WavMarker((Occlusive)Recline.Occlusives[i / 2], pos);
                else line = new WavMarker(new Occlusive("{Co}"), pos);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Cos.Remove(pos);
                    DrawConfig();
                };
            }
        }
        void DrawCf()
        {
            for (int i = 0; i < Cfs.Count; i++)
            {
                double pos = Cfs[i];
                WavMarker line;
                if (i / 2 < Recline.Fricatives.Count)
                    line = new WavMarker((Frivative)Recline.Fricatives[i / 2], pos, i % 2);
                else line = new WavMarker(new Frivative("{Cf}"), pos, i % 2);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Cfs.Remove(pos);
                    DrawConfig();
                };
            }
            for (int i = 0; i + 1 < Cfs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = CfZoneBrush,
                    Points = new PointCollection
                    {
                        new Point((Cfs[i]) * ScaleX, 50),
                        new Point((Cfs[i] + 0.01) * ScaleX, 40),
                        new Point((Cfs[i + 1] - 0.01) * ScaleX, 40),
                        new Point((Cfs[i + 1]) * ScaleX, 50),
                        new Point((Cfs[i + 1] - 0.01) * ScaleX, 60),
                        new Point((Cfs[i] + 0.01) * ScaleX, 60)
                    }
                };
                AreasCanvas.Children.Add(Zone);
            }
        }

        public string Generate()
        {
            Normalize();
            List<WavMarker> markers = MarkerCanvas.Children.OfType<WavMarker>().ToList();
            markers.OrderBy(n => n.Position);
            string text = "";
            var phonemes = Recline.Phonemes;
            Phoneme In = new Rest("-") { Marker = Data[0] };
            Phoneme Out = new Rest("-") { Marker = Data[1] };
            text += phonemes[0].GetMonophone(Recline.Filename);
            if (phonemes.Count > 1) text += phonemes[0].GetDiphone(Recline.Filename, In);
            if (phonemes.Count > 2) text += phonemes[0].GetTriphone(Recline.Filename, phonemes[0], In);
            int i;
            for (i = 2; i < Recline.Phonemes.Count; i++)
            {
                text += phonemes[i].GetMonophone(Recline.Filename);
                text += phonemes[i].GetDiphone(Recline.Filename, phonemes[i - 1]);
                text += phonemes[i].GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
            }
            text += Out.GetMonophone(Recline.Filename);
            text += Out.GetDiphone(Recline.Filename, phonemes[i - 1]);
            text += Out.GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
            return text;
        }

        void Normalize()
        {
            if (Cfs.Count > 0)
            {

            }
            if (Cos.Count > 0)
            {

            }
        }
    }
}
