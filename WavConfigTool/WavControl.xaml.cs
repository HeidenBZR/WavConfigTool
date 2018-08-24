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
        C
    }
    /// <summary>
    /// Логика взаимодействия для WavControl.xaml
    /// </summary>
    public partial class WavControl : UserControl
    {
        public Recline Recline;
        public List<double> Cs;
        public List<double> Vs;
        public List<double> Ds;

        public WavMarker[] Data = new WavMarker[2]; 

        public static double ScaleX = 0.7f;
        public static int SampleRate = 44100;
        public static double ScaleY = 100f;
        public static int PointSkip = 5;
        public static double MostLeft = 999999;

        public static int VFade = 100;
        public static int CFade = 30;
        public static int DFade = 200;

        SolidColorBrush CutZoneBrush = new SolidColorBrush(Color.FromArgb(90, 200, 100, 100));
        SolidColorBrush VowelZoneBrush = new SolidColorBrush(Color.FromArgb(250, 200, 50, 200));
        SolidColorBrush CZoneBrush = new SolidColorBrush(Color.FromArgb(200, 250, 250, 50));
        SolidColorBrush WavZoneBrush = new SolidColorBrush(Color.FromArgb(250, 100, 200, 100));

        ImageSource Source;

        public WavControl(Recline recline)
        {
            Recline = recline;
            InitializeComponent();
            Cs = new List<double>();
            Vs = new List<double>();
            Ds = new List<double>();
            LabelName.Content = $"{recline.Description} [{String.Join(" ", recline.Phonemes.Select(n => n.Alias))}]";
        }


        override protected void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            double x = e.GetPosition(this).X;
            if (Keyboard.IsKeyDown(Key.V))
                Draw(WavConfigPoint.V, x);
            else if (Keyboard.IsKeyDown(Key.C))
                Draw(WavConfigPoint.C, x);
            else if (Keyboard.IsKeyDown(Key.D))
                Draw(WavConfigPoint.D, x);
        }

        public void Draw()
        {
            if (Source != null)
            {
                DrawConfig();
                return;
            }
            AudioFileReader reader = new AudioFileReader(Recline.Path);
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
                    line.Points.Add(new Point(i * ScaleX / SampleRate * 1000, data[i] * ScaleY + 50));
                else
                {
                    line.Points.Add(new Point(i * ScaleX / SampleRate * 1000, 50));
                }
                if (data[i] >= max * 0.1)
                {
                    if (Ds.Count == 0)
                    {
                        Ds.Add((double)i / SampleRate * 1000);
                        if (Ds[0] * ScaleX < MostLeft) MostLeft = Ds[0] * ScaleX;
                    }
                        
                    else
                        lastpoint = i;
                }
            }
            if (Ds.Count < 2) Ds.Add((double)lastpoint / SampleRate * 1000);
            line.SnapsToDevicePixels = true;
            line.Stroke = WavZoneBrush;
            Height = 100;
            Width = line.Points.Last().X;
            reader.Close();
            Source = ToImageSource(line, Width, Height);
            line.Points.Clear();
            Image image = new Image() { Source = Source };
            WavCanvas.Children.Add(image);
            Background = new SolidColorBrush(Color.FromArgb(20, 90, 200, 200));
            Margin = new Thickness(0, 10, 0, 10);
            HorizontalAlignment = HorizontalAlignment.Left;
            DrawConfig();
        }


        void Draw(WavConfigPoint point, double x)
        {
            switch (point)
            {
                case WavConfigPoint.C:
                    Cs.Add(x / ScaleX);
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
            Cs.Sort();
            AreasCanvas.Children.Clear();
            MarkerCanvas.Children.Clear();
            DrawD();
            DrawV();
            DrawC();

            DrawVZone();
            DrawCZone();
            DrawDZone();
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
                line.Margin = new Thickness(pos * ScaleX, 0, 0, 0);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Vs.Remove(pos);
                    DrawConfig();
                };
            }
        }
        void DrawD()
        {
            for (int i = 0; i < Ds.Count; i++)
            {
                double pos = Ds[i];
                WavMarker line = new WavMarker(pos, i);
                Data[i] = line;
                line.Margin = new Thickness(pos * ScaleX, 0, 0, 0);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Ds.Remove(pos);
                    DrawConfig();
                };
            }
        }
        void DrawC()
        {
            for (int i = 0; i < Cs.Count; i++)
            {
                double pos = Cs[i];
                WavMarker line;
                if (i / 2 < Recline.Consonants.Count)
                    line = new WavMarker((Consonant)Recline.Consonants[i / 2], pos, i % 2);
                else line = new WavMarker(new Consonant("{Cf}"), pos, i % 2);
                line.Margin = new Thickness(pos * ScaleX, 0, 0, 0);
                MarkerCanvas.Children.Add(line);
                line.MouseRightButtonUp += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Cs.Remove(pos);
                    DrawConfig();
                };
            }
        }

        void DrawVZone()
        {
            for (int i = 0; i + 1 < Vs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = VowelZoneBrush,
                    Points = new PointCollection
                    {
                        new Point((Vs[i]) * ScaleX, 50),
                        new Point((Vs[i] + VFade) * ScaleX, 30),
                        new Point((Vs[i + 1] - VFade) * ScaleX, 30),
                        new Point((Vs[i + 1]) * ScaleX, 50),
                        new Point((Vs[i + 1] - VFade) * ScaleX, 70),
                        new Point((Vs[i] + VFade) * ScaleX, 70)
                    }
                };
                AreasCanvas.Children.Add(Zone);
            }
        }
        void DrawDZone()
        {
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
                    new Point(x - DFade * ScaleX,50),
                    new Point(x,100),
                    new Point(0,100)
                },
                Opacity = 0.5
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
                    new Point(x + DFade * ScaleX,50),
                    new Point(x,100),
                    new Point(Width,100)
                },
                Opacity = 0.5
            };
            AreasCanvas.Children.Add(ZoneOut);
        }
        void DrawCZone()
        {

            for (int i = 0; i + 1 < Cs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = CZoneBrush,
                    Points = new PointCollection
                    {
                        new Point((Cs[i]) * ScaleX, 50),
                        new Point((Cs[i] + CFade) * ScaleX, 40),
                        new Point((Cs[i + 1] - CFade) * ScaleX, 40),
                        new Point((Cs[i + 1]) * ScaleX, 50),
                        new Point((Cs[i + 1] - CFade) * ScaleX, 60),
                        new Point((Cs[i] + CFade) * ScaleX, 60)
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
            Phoneme In = new Rest("-");
            Phoneme Out = new Rest("-");
            In.Zone.In = Data[0];
            In.Zone.Out = Data[0];
            In.Recline = Recline;
            Out.Zone.In = Data[1];
            Out.Zone.Out = Data[1];
            Out.Recline = Recline;
            text += phonemes[0].GetMonophone(Recline.Filename);
            if (phonemes.Count > 1) text += phonemes[0].GetDiphone(Recline.Filename, In);
            if (phonemes.Count > 1) text += phonemes[1].GetDiphone(Recline.Filename, phonemes[0]);
            if (phonemes.Count > 2) text += phonemes[1].GetTriphone(Recline.Filename, phonemes[0], In);
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

        /// <summary>
        /// Не реализовано!
        /// </summary>
        void Normalize()
        {
            // Check that all Vowels ans Consonants are inside Data; and Vowels & Consonants doesn't intersect
            if (Cs.Count > 0) { }
        }

        public ImageSource ToImageSource(FrameworkElement obj, double w, double h)
        {
            // Save current canvas transform
            Transform transform = obj.LayoutTransform;
            obj.LayoutTransform = null;

            // fix margin offset as well
            Thickness margin = obj.Margin;
            obj.Margin = new Thickness(0, 0,
                 margin.Right - margin.Left, margin.Bottom - margin.Top);

            // Get the size of canvas
            Size size = new Size(w, h);

            // force control to Update
            obj.Measure(size);
            obj.Arrange(new Rect(size));

            RenderTargetBitmap bmp = new RenderTargetBitmap(
                (int)w, (int)h, 96, 96, PixelFormats.Pbgra32);

            bmp.Render(obj);

            // return values as they were before
            obj.LayoutTransform = transform;
            obj.Margin = margin;
            return bmp;
        }
    }
}
