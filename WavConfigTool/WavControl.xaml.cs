using System;
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
        Task GeneratingTask;
        CancellationTokenSource CancellationToken = new CancellationTokenSource();

        public Recline Recline;
        List<double> _cs = new List<double>();
        List<double> _vs = new List<double>();
        List<double> _ds = new List<double>();
        public List<double> Cs { get { return _cs; } set { _cs = value; ApplyPoints(WavConfigPoint.C); CheckCompleted(); } }
        public List<double> Vs { get { return _vs; } set { _vs = value; ApplyPoints(WavConfigPoint.V); CheckCompleted(); } }
        public List<double> Ds { get { return _ds; } set { _ds = value; ApplyPoints(WavConfigPoint.D); CheckCompleted(); } }

        public WavMarker[] Data = new WavMarker[2];
        public string ImagePath { get {
                var c = AudioCode;
                return System.IO.Path.Combine(MainWindow.TempDir, $"{c}_{Recline.Filename}.png"); } }

        public static double ScaleX = 0.7f;
        public static double ScaleY = 60f;
        public static double MostLeft = 9999;

        public static string Prefix;
        public static string Suffix;

        public int Length;
        public bool IsCompleted;
        public bool IsGenerating = false;
        public bool IsEnabled = false;

        SolidColorBrush CutZoneBrush = new SolidColorBrush(Color.FromArgb(250, 2, 20, 4));
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
            IsEnabled = File.Exists(Recline.Path);
                
        }

        void CheckCompleted()
        {
            IsCompleted = Ds.Count == 2 &&
                Vs.Count / 2 >= Recline.Vowels.Count &&
                Cs.Count / 2 >= Recline.Consonants.Count;
            Dispatcher.BeginInvoke((ThreadStart)delegate
           {
               if (IsCompleted) WavCompleted.Opacity = 1;
               else WavCompleted.Opacity = 0;
           });
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
            Normalize();
            ApplyPoints();
            ApplyFade();
            List<WavMarker> markers = MarkerCanvas.Children.OfType<WavMarker>().ToList();
            markers.OrderBy(n => n.Position);
            string text = "";
            string[] iy = new[] { "iy", "yi" };
            var phonemes = Recline.Phonemes;
            if (Recline.Consonants.Count < Recline.Vowels.Count) text += phonemes[0].GetMonophone(Recline.Filename);
            if ((phonemes.Count > 1 && phonemes[0].IsVowel) 
                || (phonemes.Count > 2 && phonemes[1].Alias == "a"))
                text += phonemes[0].GetDiphone(Recline.Filename, Recline.Data[0]);
            if (phonemes.Count > 2 && phonemes[1].Alias == "a") text += Recline.Data[1].GetDiphone(Recline.Filename, phonemes.Last());
            if (phonemes.Count > 1 && Recline.Consonants.Count < Recline.Vowels.Count) text += phonemes[1].GetDiphone(Recline.Filename, phonemes[0]);
            if (phonemes.Count > 1 && !(phonemes.Count > 3 && iy.Contains(phonemes[1] + phonemes[3]))) text += phonemes[1].GetTriphone(Recline.Filename, phonemes[0], Recline.Data[0]);
            int i;
            for (i = 2; i < Recline.Phonemes.Count; i++)
            {
                if (Recline.Consonants.Count < Recline.Vowels.Count) text += phonemes[i].GetMonophone(Recline.Filename);
                if ( i != 4 || !Reclist.Current.Name.ToLower().Contains("cvc"))
                    text += phonemes[i].GetDiphone(Recline.Filename, phonemes[i - 1]);
                text += phonemes[i].GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
            }
            if (phonemes.Count == 1) i = 1;
            //text += Recline.Data[1].GetMonophone(Recline.Filename);
            if (phonemes.First().IsVowel) text += Recline.Data[1].GetDiphone(Recline.Filename, phonemes.Last());
            if (phonemes.Count > 1) text += Recline.Data[1].GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
            return text;
        }


        void ApplyFade()
        {
            foreach (var phoneme in Recline.Phonemes)
            {
                int fade;
                if (phoneme.Type == PhonemeType.Consonant) fade = Settings.FadeC;
                else if (phoneme.Type == PhonemeType.Vowel) fade = Settings.FadeV;
                else fade = Settings.FadeD;
                phoneme.Fade = fade;
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

        void ApplyPoints()
        {
            ApplyPoints(WavConfigPoint.C);
            ApplyPoints(WavConfigPoint.V);
            ApplyPoints(WavConfigPoint.D);
        }
        void ApplyPoints(WavConfigPoint point)
        {
            if (point == WavConfigPoint.C)
            {
                for (int i = 0; i < Cs.Count; i++)
                {
                    if (i % 2 == 0)
                        Recline.Consonants[i / 2].Zone.In = Cs[i];
                    else
                        Recline.Consonants[i / 2].Zone.Out = Cs[i];
                }
            }
            if (point == WavConfigPoint.V)
            {
                for (int i = 0; i < Vs.Count; i++)
                {
                    if (i % 2 == 0)
                        Recline.Vowels[i / 2].Zone.In = Vs[i];
                    else
                        Recline.Vowels[i / 2].Zone.Out = Vs[i];
                }
            }
            if (point == WavConfigPoint.D)
            {
                Recline.Data = new List<Phoneme>();
                foreach (var d in Ds)
                {
                    Phoneme phoneme = new Rest("-", d, Recline);
                    Recline.Data.Add(phoneme);
                }
            }
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
            if (!IsEnabled)
                return;
            Opacity = 0.2;
            OpenImageAsync();
            DrawConfig();
            CheckCompleted();
        }

        public void Undraw()
        {
            AreasCanvas.Children.Clear();
            GridCanvas.Children.Clear();
            MarkerCanvas.Children.Clear();
        }

        public bool GenerateWaveform(bool force = false)
        {
            if (!File.Exists(Recline.Path))
                return false;
            if (!force && File.Exists(ImagePath))
                return true;
            while (IsGenerating)
            {
                // Wait for previous generating end;
                Thread.Sleep(10);
            }
            IsGenerating = true;
            WaveForm wave = new WaveForm(Recline.Path);
            var points = wave.GetAudioPoints();
            if (Ds == null || Ds.Count == 0)
                Ds = new List<double>();
                //Ds = wave.Ds;
            MostLeft = wave.MostLeft;
            Length = (int) (wave.Length * 1000 / wave.SampleRate);
            
            int width = (int) points.Last().X;
            Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                Width = width;
                Height = 100;
            });

            if (!wave.IsEnabled)
                return false;
            wave.PointsToImage((object)(points, width, 100, this));
            IsGenerating = false;
            if (!wave.IsGenerated)
            {
                MessageBox.Show($"{wave.GeneratingException.Message}\r\n\r\n{wave.GeneratingException.StackTrace}", 
                    "Error on image generation", MessageBoxButton.OK, MessageBoxImage.Error);
                return false;
            }
            return true;
        }


        public async void GenerateWaveformAsync(bool force)
        {
            if (!IsEnabled)
                return;
            GeneratingTask = new Task(delegate() {
                IsEnabled = GenerateWaveform(force);
            });
            GeneratingTask.Start();
            await GeneratingTask;
        } 

        public async void OpenImageAsync()
        {
            if (!IsEnabled)
                return;

            int i = 0;

            await Task.Run(() =>
            {


                while (!File.Exists(ImagePath) || IsGenerating)
                {
                    // Wait for previous generating end;
                    Thread.Sleep(100);
                    i++;
                    if (i > 100)
                        return;
                }

                OpenImage();
                Dispatcher.Invoke(() =>
                {
                    Opacity = 1;
                });
            });
        }

        public void OpenImage()
        {
            BitmapImage src = new BitmapImage();
            src.BeginInit();
            src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            src.UriSource = new Uri(ImagePath, UriKind.Relative);
            src.CacheOption = BitmapCacheOption.OnLoad;
            src.EndInit();
            src.Freeze();
            Dispatcher.BeginInvoke((ThreadStart)delegate
            {
                WavImage.Source = src;
            });
            //var uri = new Uri(ImagePath, UriKind.Relative);
            //WavImage.Source = new BitmapImage(uri);
        }


        void Draw(WavConfigPoint point, double x)
        {
            switch (point)
            {
                case WavConfigPoint.C:
                    if (Cs.Count < Recline.Consonants.Count * 2)
                        Cs.Add(x / ScaleX);
                    break;
                case WavConfigPoint.V:
                    if (Vs.Count < Recline.Vowels.Count * 2)
                        Vs.Add(x / ScaleX);
                    break;
                case WavConfigPoint.D:
                    if (Ds.Count < 2)
                        Ds.Add(x / ScaleX);
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
            if (Ds != null && Ds.Count == 2)
            {
                if (Ds[0] < 0)
                    Ds[0] = 50;
                if (Ds[1] > Length)
                    Ds[1] = Length - 50;
            }
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
                        new Point((Vs[i] + Settings.FadeV) * ScaleX, 30),
                        new Point((Vs[i + 1] - Settings.FadeV) * ScaleX, 30),
                        new Point((Vs[i + 1]) * ScaleX, 50),
                        new Point((Vs[i + 1] - Settings.FadeV) * ScaleX, 70),
                        new Point((Vs[i] + Settings.FadeV) * ScaleX, 70)
                    },
                    Name = $"VZone{i / 2}{(i % 2 == 0? "In":"Out")}",
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
                    new Point(x - Settings.FadeD * ScaleX,50),
                    new Point(x,100),
                    new Point(0,100)
                },
                Name = "DZoneIn",
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
                    new Point(Length * ScaleX,0),
                    new Point(x,0),
                    new Point(x + Settings.FadeD * ScaleX,50),
                    new Point(x,100),
                    new Point(Length * ScaleX,100)
                },
                Name = "DZoneOut",
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
                        new Point((Cs[i] + Settings.FadeC) * ScaleX, 40),
                        new Point((Cs[i + 1] - Settings.FadeC) * ScaleX, 40),
                        new Point((Cs[i + 1]) * ScaleX, 50),
                        new Point((Cs[i + 1] - Settings.FadeC) * ScaleX, 60),
                        new Point((Cs[i] + Settings.FadeC) * ScaleX, 60)
                    },
                    Name = $"CZone{i / 2}{(i % 2 == 0 ? "In" : "Out")}",
                    Fill = FillCZoneBrush
                };
                AreasCanvas.Children.Add(Zone);
            }
        }


        #endregion

        #region Events

        private void WavControl_Reset(object sender, RoutedEventArgs e)
        {
            string tag = (sender as MenuItem).Tag.ToString();
            if (tag == "All") Reset();
            else if (tag == "C") Reset(WavConfigPoint.C);
            else if (tag == "V") Reset(WavConfigPoint.V);
            else if (tag == "D") Reset(WavConfigPoint.D);
        }

        private void WavControl_OtoPreview(object sender, RoutedEventArgs e)
        {
            DrawOtoPreview();
        }

        private void WavCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
                DrawOtoPreview();
            else
            {
                if (!WavContextMenu.IsVisible && Keyboard.IsKeyUp(Key.Space))
                {
                    double x = e.GetPosition(this).X;
                    Draw(MainWindow.Mode, x);
                    WavControlChanged();
                }
            }
        }

        #endregion
    }
}
