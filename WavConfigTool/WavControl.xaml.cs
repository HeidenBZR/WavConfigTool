﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
        List<double> _cs = new List<double>();
        List<double> _vs = new List<double>();
        List<double> _ds = new List<double>();
        public List<double> Cs { get { return _cs; } set { _cs = value; CheckCompleted(); } }
        public List<double> Vs { get { return _vs; } set { _vs = value; CheckCompleted(); } }
        public List<double> Ds { get { return _ds; } set { _ds = value; CheckCompleted(); } }

        public WavMarker[] Data = new WavMarker[2];
        public string ImagePath { get { return System.IO.Path.Combine("Temp", $"{AudioCode}_{Recline.Filename}.png"); } }

        public static double ScaleX = 0.7f;
        public static int SampleRate = 44100;
        public static double ScaleY = 60f;
        public static int PointSkip = 5;
        public static double MostLeft = 9999;
        public static double WaveformAmplitudeMultiplayer = 1f;

        public static int VFade = 30;
        public static int CFade = 10;
        public static int DFade = 200;
        public static string Prefix;
        public static string Suffix;

        public int Length;
        public bool IsCompleted;

        SolidColorBrush CutZoneBrush = new SolidColorBrush(Color.FromArgb(90, 200, 100, 100));
        SolidColorBrush VowelZoneBrush = new SolidColorBrush(Color.FromArgb(250, 200, 200, 50));
        SolidColorBrush CZoneBrush = new SolidColorBrush(Color.FromArgb(250, 50, 250, 250));
        SolidColorBrush FillVowelZoneBrush = new SolidColorBrush(Color.FromArgb(50, 200, 200, 50));
        SolidColorBrush FillCZoneBrush = new SolidColorBrush(Color.FromArgb(50, 50, 250, 250));

        public delegate void WavControlChangedHandler();
        public event WavControlChangedHandler WavControlChanged;

        public bool IsImageGenerated { get { return File.Exists(ImagePath); } }

        public string AudioCode
        {
            get
            {
                return new DirectoryInfo(Recline.Reclist.VoicebankPath).Name;
            }
        }

        public WavControl(Recline recline)
        {
            Recline = recline;
            InitializeComponent();
            Cs = new List<double>();
            Vs = new List<double>();
            Ds = new List<double>();
            LabelName.Content = recline.Name;
            WavControlChanged += delegate { CheckCompleted(); };
        }

        void CheckCompleted()
        {
            IsCompleted = Ds.Count == 2 &&
                Vs.Count / 2 >= Recline.Vowels.Count &&
                Cs.Count / 2 >= Recline.Consonants.Count;
            if (IsCompleted) WavCompleted.Opacity = 1;
            else WavCompleted.Opacity = 0;
        }

        void Reset()
        {
            Ds = new List<double>();
            Vs = new List<double>();
            Cs = new List<double>();
            DrawConfig();
            WavControlChanged();
        }
        void Reset(WavConfigPoint point)
        {
            if (point == WavConfigPoint.C) Cs = new List<double>();
            if (point == WavConfigPoint.V) Vs = new List<double>();
            if (point == WavConfigPoint.D) Ds = new List<double>();
            DrawConfig();
            WavControlChanged();
        }

        public string Generate()
        {
            DrawConfig();
            ApplyFade();
            Normalize();
            List<WavMarker> markers = MarkerCanvas.Children.OfType<WavMarker>().ToList();
            markers.OrderBy(n => n.Position);
            string text = "";
            var phonemes = Recline.Phonemes;
            Phoneme In = new Rest("-");
            Phoneme Out = new Rest("-");
            In.FadeIn = DFade;
            In.FadeOut = DFade;
            In.Zone.In = Data[0];
            In.Zone.Out = Data[0];
            In.Recline = Recline;
            Out.FadeIn = DFade;
            Out.FadeOut = DFade;
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


        void ApplyFade()
        {
            foreach (var phoneme in Recline.Phonemes)
            {
                int fade;
                if (phoneme.Type == PhonemeType.Consonant) fade = CFade;
                else if (phoneme.Type == PhonemeType.Vowel) fade = VFade;
                else fade = DFade;
                phoneme.FadeIn = fade;
                phoneme.FadeOut = fade;
            }
        }
        /// <summary>
        /// Не реализовано!
        /// </summary>
        void Normalize()
        {
            // Check that all Vowels ans Consonants are inside Data; and Vowels & Consonants doesn't intersect
            if (Cs.Count > 0) { }
        }
        
        void DrawOtoPreview()
        {
            Recline.Reclist.Aliases = new List<string>();
            string oto = Generate();
            OtoPreviewWindow window = new OtoPreviewWindow(Recline.Filename);
            foreach (string line in oto.Split(new[] { '\r', '\n' }))
            {
                if (line.Length == 0) continue;
                var ops = line.Split('=');
                var ops2 = ops[1].Split(',');
                var ops3 = ops2.Skip(1);
                int[] opsi = ops3.Select(n => int.Parse(n)).ToArray();
                OtoPreviewControl control = new OtoPreviewControl(WavImage.Source, ops2[0], opsi, Length);
                window.Add(control);
            }
            window.ShowDialog();
        }

        #region Draw

        public void Draw()
        {
            if (!File.Exists(ImagePath)) GenerateWaveform();
            Display();
            DrawConfig();
            CheckCompleted();
        }

        public void Undraw()
        {
            AreasCanvas.Children.Clear();
            GridCanvas.Children.Clear();
            MarkerCanvas.Children.Clear();
        }

        void Display()
        {
            OpenImage();
        }

        public void GenerateWaveform(bool force = false)
        {
            if (!force && File.Exists(ImagePath)) return;
            var points = GetAudioPoints();
            Width = points.Last().X;
            Height = 100;


            Thread thread = new Thread(WaveForm.PointsToImage);
            thread.Name = Recline.Filename;
            thread.Start((points, (int)Width, (int)Height, this));
            if (force) Undraw();
        }


        void OpenImage()
        {
            bool isAlowed = false;
            for (int i = 0; !IsImageGenerated && !isAlowed; i++)
            {
                i++;
                Thread.Sleep(100);
                if (i > 1000) throw new Exception("It's beeeeen too loooong~");
                try { var f = File.Open(ImagePath, FileMode.Open); isAlowed = true; f.Close(); }
                catch (IOException) { isAlowed = false; }
            }
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            src.UriSource = new Uri(ImagePath, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            WavImage.Source = src;
            Width = src.Width;
        }

        Point[] GetAudioPoints()
        {
            AudioFileReader reader = new AudioFileReader(Recline.Path);
            SampleRate = reader.WaveFormat.SampleRate;
            var l = reader.Length;
            float[] data = new float[l];
            reader.Read(data, 0, (int)l);
            List<Point> points = new List<Point>();
            long lastpoint = 0;
            var max = data.Max();
            long i = 0;
            for ( ; i < l / 4; i += PointSkip)
            {
                if (Math.Abs(data[i]) > 0.001)
                    points.Add(new Point(i * ScaleX / SampleRate * 1000, data[i] * ScaleY * WaveformAmplitudeMultiplayer + 50));
                else
                {
                    points.Add(new Point(i * ScaleX / SampleRate * 1000, 50));
                }
                if (data[i] >= max * 0.05)
                {
                    if (Ds.Count == 0)
                    {
                        Ds.Add((double)i / SampleRate * 1000 * WaveformAmplitudeMultiplayer);
                    }
                    else
                    {
                        if (Ds[0] * ScaleX < MostLeft) MostLeft = Ds[0] * ScaleX;
                        lastpoint = i;
                    }
                }
            }
            Length = (int)(i * 1000 / SampleRate);
            if (Ds.Count < 2) Ds.Add((double)lastpoint / SampleRate * 1000 * WaveformAmplitudeMultiplayer);
            reader.Close();
            return points.ToArray();
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
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Vs.Remove(pos);
                    DrawConfig();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Vs[Vs.IndexOf(pos)] = Math.Truncate(newpos);
                    DrawConfig();
                    WavControlChanged();
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
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Cs.Remove(pos);
                    DrawConfig();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Cs[Cs.IndexOf(pos)] = Math.Truncate(newpos);
                    DrawConfig();
                    WavControlChanged();
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
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Ds.Remove(pos);
                    DrawConfig();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Ds[Ds.IndexOf(pos)] = Math.Truncate(newpos);
                    DrawConfig();
                    WavControlChanged();
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
                    },
                    Fill = FillVowelZoneBrush
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
                    },
                    Fill = FillCZoneBrush
                };
                AreasCanvas.Children.Add(Zone);
            }
        }


        #endregion

        #region Events

        override protected void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                double x = e.GetPosition(this).X;
                Draw(MainWindow.Mode, x);
                WavControlChanged();
            }
        }

        private void WavControl_Reset(object sender, RoutedEventArgs e)
        {
            string tag = (sender as MenuItem).Tag.ToString();
            if (tag == "All") Reset();
            else if (tag == "C") Reset(WavConfigPoint.C);
            else if (tag == "V") Reset(WavConfigPoint.V);
            else if (tag == "D") Reset(WavConfigPoint.D);
        }

        private void UserControl_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (Keyboard.IsKeyDown(Key.R))
                    Reset();
            }
        }

        private void OtoPreview_Click(object sender, RoutedEventArgs e)
        {
            DrawOtoPreview();
        }

        #endregion

    }
}
