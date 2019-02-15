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
        public Thread Thread;

        public Recline Recline;
        List<double> _cs = new List<double>();
        List<double> _vs = new List<double>();
        List<double> _ds = new List<double>();
        public List<double> Cs { get { return _cs; } set { _cs = value; ApplyPoints(WavConfigPoint.C); CheckCompleted(); } }
        public List<double> Vs { get { return _vs; } set { _vs = value; ApplyPoints(WavConfigPoint.V); CheckCompleted(); } }
        public List<double> Ds { get { return _ds; } set { _ds = value; ApplyPoints(WavConfigPoint.D); CheckCompleted(); } }

        public WavMarker[] Data = new WavMarker[2];
        public string ImagePath;

        public string GetImagePath()
        {
            var c = AudioCode;
            var to_hash = $"{c};{Recline.Filename};{Settings.WAM}";
            var hex = $"{to_hash.GetHashCode():X}";
            return System.IO.Path.Combine(MainWindow.TempDir, $"{hex}.png");
        }

        public static double ScaleX = 0.7;
        public static double ScaleY = 60;
        public static double MostLeft = 0;
        public static double WavHeight = 100;

        public static string Prefix;
        public static string Suffix;

        public static int WaitingLimit = 10;

        public WaveForm WaveForm;

        public bool IsToDraw = false;
        public int Length;
        public bool IsCompleted;
        public bool IsGenerating
        {
            get { return WaveForm is null ? false : WaveForm.IsGenerating; }
        }

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
            //Visibility = Visibility.Hidden;
            Cs = new List<double>();
            Vs = new List<double>();
            Ds = new List<double>();
            LabelName.Content = recline.Name;
            WavControlChanged += delegate { CheckCompleted(); };
            OnImageLoaded += delegate {
                SetLoaded();
                DrawConfig();
                CheckCompleted();
            };
        }

        public bool InitWave()
        {
            try
            {
                if (Recline is null || !Recline.IsEnabled)
                    return false;

                WaveForm = new WaveForm(Recline.Path);
                WaveForm.IsGenerated = true;
                MostLeft = WaveForm.MostLeft;

                Length = (int)(WaveForm.Length * 1000 / WaveForm.SampleRate);

                if (!WaveForm.IsEnabled)
                    return false;
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on Init Wave");
            }
            return false;
        }


        void Reset()
        {
            Ds = new List<double>();
            Vs = new List<double>();
            Cs = new List<double>();
            Draw();
            WavControlChanged();
        }
        void Reset(WavConfigPoint point)
        {
            if (point == WavConfigPoint.C) Cs = new List<double>();
            if (point == WavConfigPoint.V) Vs = new List<double>();
            if (point == WavConfigPoint.D) Ds = new List<double>();
            Draw();
            WavControlChanged();
        }

        public string GenerateOto()
        {
            if (!Recline.IsEnabled)
                return "";
            Normalize();
            ApplyPoints();
            ApplyFade();
            string text = "";
            int i;
            try
            {

                List<WavMarker> markers = MarkerCanvas.Children.OfType<WavMarker>().ToList();
                markers.OrderBy(n => n.Position);
                string[] iy = new[] { "iy", "yi" };
                var phonemes = Recline.Phonemes;
                if (Recline.Consonants.Count < Recline.Vowels.Count) text += phonemes[0].GetMonophone(Recline.Filename);
                if (Recline.Data.Count > 0)
                {
                    if (Recline.Reclist.Name.Contains("cvc_rus") &&
                            ((phonemes.Count > 1 && phonemes[0].IsVowel)
                            || (phonemes.Count > 2 && phonemes[1].Alias == "a")))
                        text += phonemes[0].GetDiphone(Recline.Filename, Recline.Data[0]);
                    else
                        text += phonemes[0].GetDiphone(Recline.Filename, Recline.Data[0]);
                }
                if (phonemes.Count > 2 && (!Recline.Reclist.Name.Contains("cvc_rus") || phonemes[1].Alias == "a")) text += Recline.Data[1].GetDiphone(Recline.Filename, phonemes.Last());
                if (phonemes.Count > 1 && (!Recline.Reclist.Name.Contains("cvc_rus") || Recline.Consonants.Count < Recline.Vowels.Count)) text += phonemes[1].GetDiphone(Recline.Filename, phonemes[0]);
                if (Recline.Data.Count > 0)
                    if (phonemes.Count > 1 && (!Recline.Reclist.Name.Contains("cvc_rus") || !(phonemes.Count > 3 && iy.Contains(phonemes[1] + phonemes[3]))))
                        text += phonemes[1].GetTriphone(Recline.Filename, phonemes[0], Recline.Data[0]);
                for (i = 2; i < Recline.Phonemes.Count; i++)
                {
                    if (Recline.Consonants.Count < Recline.Vowels.Count) text += phonemes[i].GetMonophone(Recline.Filename);
                    if (i != 4 || !Reclist.Current.Name.ToLower().Contains("cvc_rus"))
                        text += phonemes[i].GetDiphone(Recline.Filename, phonemes[i - 1]);
                    text += phonemes[i].GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
                }
                if (phonemes.Count == 1) i = 1;
                //text += Recline.Data[1].GetMonophone(Recline.Filename);
                if (Recline.Data.Count > 1)
                {
                    if (phonemes.First().IsVowel) text += Recline.Data[1].GetDiphone(Recline.Filename, phonemes.Last());
                    if (phonemes.Count > 1) text += Recline.Data[1].GetTriphone(Recline.Filename, phonemes[i - 1], phonemes[i - 2]);
                }
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on draw page");
            }
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
                    if (i / 2 < Recline.Consonants.Count)
                    {
                        if (i % 2 == 0)
                            Recline.Consonants[i / 2].Zone.In = Cs[i];
                        else
                            Recline.Consonants[i / 2].Zone.Out = Cs[i];
                    }
                }
            }
            if (point == WavConfigPoint.V)
            {
                for (int i = 0; i < Vs.Count; i++)
                {
                    if (i / 2 < Recline.Vowels.Count)
                    {
                        if (i % 2 == 0)
                            Recline.Vowels[i / 2].Zone.In = Vs[i];
                        else
                            Recline.Vowels[i / 2].Zone.Out = Vs[i];

                    }
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
                }
            }
        }

        #endregion
    }
}
