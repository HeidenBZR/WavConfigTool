﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.UserControls
{
    public partial class WavControl 
    {


        //Task GeneratingTask;
        //public Thread Thread;

        //public Recline Recline;
        //List<double> _cs = new List<double>();
        //List<double> _vs = new List<double>();
        //List<double> _ds = new List<double>();
        //public List<double> Cs { get { return _cs; } set { _cs = value; ApplyPoints(WavConfigPoint.C); CheckCompleted(); } }
        //public List<double> Vs { get { return _vs; } set { _vs = value; ApplyPoints(WavConfigPoint.V); CheckCompleted(); } }
        //public List<double> Ds { get { return _ds; } set { _ds = value; ApplyPoints(WavConfigPoint.D); CheckCompleted(); } }

        //public WavMarker[] Data = new WavMarker[2];
        //public string ImagePath;

        //public string GetImagePath()
        //{
        //    var c = AudioCode;
        //    var to_hash = $"{c};{Recline.Filename};{Settings.WAM}";
        //    var hex = $"{to_hash.GetHashCode():X}";
        //    return System.IO.Path.Combine(MainWindow.TempDir, $"{hex}.png");
        //}

        //public static double MostLeft = 0;
        //public static double WavHeight = 100;


        //public static int WaitingLimit = 100;
        //public bool AllowOtoPreview { get; set; } = true;

        //public WaveForm WaveForm;

        //public bool IsToDraw = false;
        //public int Length;
        //public bool IsCompleted;
        //public bool IsGenerating
        //{
        //    get { return WaveForm is null ? false : WaveForm.IsGenerating; }
        //}

        //SolidColorBrush FillRestSustainBrush = new SolidColorBrush(Color.FromArgb(255, 2, 20, 4));
        //SolidColorBrush VowelZoneBrush = new SolidColorBrush(Color.FromArgb(255, 200, 200, 50));
        //SolidColorBrush ConsonantZoneBrush = new SolidColorBrush(Color.FromArgb(255, 50, 250, 250));
        //SolidColorBrush FillVowelZoneBrush = new SolidColorBrush(Color.FromArgb(50, 200, 200, 50));
        //SolidColorBrush FillConsonantZoneBrush = new SolidColorBrush(Color.FromArgb(50, 50, 250, 250));
        //SolidColorBrush FillVowelSustainBrush = new SolidColorBrush(Color.FromArgb(150, 170, 170, 30));
        //SolidColorBrush FillRestBrush = new SolidColorBrush(Color.FromArgb(150, 170, 30, 30));
        //SolidColorBrush FillConsonantSustainBrush = new SolidColorBrush(Color.FromArgb(150, 30, 170, 170));

        //public delegate void WavControlChangedHandler();
        //public event WavControlChangedHandler WavControlChanged;

        //public bool IsImageGenerated { get { return File.Exists(ImagePath); } }

        //public string AudioCode
        //{
        //    get
        //    {
        //        return new DirectoryInfo(Recline.Reclist.VoicebankPath).Name;
        //    }
        //}

        //public WavControl(Recline recline)
        //{
        //    Recline = recline;
        //    InitializeComponent();
        //    //Visibility = Visibility.Hidden;
        //    Cs = new List<double>();
        //    Vs = new List<double>();
        //    Ds = new List<double>();
        //    LabelName.Content = recline.Name;
        //    WavControlChanged += delegate { CheckCompleted(); };
        //    OnImageLoaded += delegate {
        //        SetLoaded();
        //        DrawConfig();
        //        CheckCompleted();
        //    };
        //}

        //public bool InitWave()
        //{
        //    try
        //    {
        //        if (Recline is null || !Recline.IsEnabled)
        //            return false;

        //        WaveForm = new WaveForm(Recline.Path);
        //        WaveForm.IsGenerated = true;
        //        MostLeft = WaveForm.MostLeft;

        //        Length = (int)(WaveForm.Length * 1000 / WaveForm.SampleRate);

        //        if (!WaveForm.IsEnabled)
        //            return false;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        MainWindow.MessageBoxError(ex, "Error on Init Wave");
        //    }
        //    return false;
        //}


        //void Reset()
        //{
        //    Ds = new List<double>();
        //    Vs = new List<double>();
        //    Cs = new List<double>();
        //    Draw();
        //    WavControlChanged();
        //}
        //void Reset(WavConfigPoint point)
        //{
        //    if (point == WavConfigPoint.C) Cs = new List<double>();
        //    if (point == WavConfigPoint.V) Vs = new List<double>();
        //    if (point == WavConfigPoint.D) Ds = new List<double>();
        //    Draw();
        //    WavControlChanged();
        //}

        //public string GenerateOto()
        //{
        //    if (!Recline.IsEnabled)
        //        return "";
        //    Normalize();
        //    ApplyPoints();
        //    ApplyFade();
        //    string text = "";
        //    int i;
        //    try
        //    {
        //        List<WavMarker> markers = MarkerCanvas.Children.OfType<WavMarker>().ToList();
        //        markers.OrderBy(n => n.Position);

        //        List<Phoneme> phonemes = new List<Phoneme>();
        //        if (Recline.Data.Count > 0)
        //            phonemes.Add(Recline.Data[0]);
        //        phonemes.AddRange(Recline.Phonemes);
        //        if (Recline.Data.Count > 1)
        //            phonemes.Add(Recline.Data[1]);

        //        for (i = 0; i < phonemes.Count; i++)
        //        {
        //            if (phonemes.Count > i + 1)
        //                text += OtoGenerator.Current.Generate(Recline.Filename, phonemes[i], phonemes[i + 1]);
        //            if (phonemes.Count > i + 2)
        //                text += OtoGenerator.Current.Generate(Recline.Filename, phonemes[i], phonemes[i + 1], phonemes[i + 2]);
        //            if (phonemes.Count > i + 3)
        //                text += OtoGenerator.Current.Generate(Recline.Filename, phonemes[i], phonemes[i + 1], phonemes[i + 2], phonemes[i + 3]);
        //            if (phonemes.Count > i + 4)
        //                text += OtoGenerator.Current.Generate(Recline.Filename, phonemes[i], phonemes[i + 1], phonemes[i + 2], phonemes[i + 3], phonemes[i + 4]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MainWindow.MessageBoxError(ex, "Error on draw page");
        //    }
        //    return text;
        //}


        //void ApplyFade()
        //{
        //    foreach (var phoneme in Recline.Phonemes)
        //    {
        //        int attack;
        //        if (phoneme.Type == PhonemeType.Consonant) attack = Settings.ConsonantAttack;
        //        else if (phoneme.Type == PhonemeType.Vowel) attack = Settings.VowelAttack;
        //        else attack = Settings.RestAttack;
        //        phoneme.Attack = attack;
        //    }
        //}
        ///// <summary>
        ///// Не реализовано!
        ///// </summary>
        //void Normalize()
        //{
        //    // Check that all Vowels ans Consonants are inside Data; and Vowels & Consonants doesn't intersect
        //    if (Cs.Count > 0) { }
        //}
    }
}