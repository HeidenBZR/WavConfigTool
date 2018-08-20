using System;
using System.Collections.Generic;
using System.IO;
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

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Reclist Reclist;
        List<WavControl> WavControls;

        public MainWindow()
        {
            InitializeComponent();
            Reclist = new Reclist();
            WavControls = new List<WavControl>();
            AddFile(@"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\#Minto Power\a.wav", "a ` a");
            AddFile(@"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\#Minto Power\kakak.wav", "k a k a k");
            AddFile(@"D:\DISCS\YandexDisk\Heiden\UTAU\_voicebanks\#Minto Power\nanan.wav", "n a n a n");
            ScrollViewer.ScrollToHorizontalOffset(WavControl.MostLeft - 0.1 * WavControl.ScaleX);
        }

        void AddFile(string filename, string phonemes)
        {
            Recline recline = new Recline(filename, phonemes);
            AddWavControl(recline);
        }
        void AddFile(string filename, string phonemes, string description)
        {
            Recline recline = new Recline(filename, phonemes, description);
            AddWavControl(recline);
        }
        void AddWavControl(Recline recline)
        {
            WavControl control = new WavControl(recline);
            Reclist.Reclines.Add(recline);
            WavControls.Add(control);
            WaveControlStackPanel.Children.Add(control);
        }

        void Save()
        {
            string text = "";
            foreach (WavControl control in WavControls)
            {
                text += $">{control.Recline.Filename}\r\n";
                text += $"D={control.Ds[0].ToString("f3")}:{control.Ds[1].ToString("f3")}\r\n";
                text += $"V={ String.Join(",", control.Vs.Select(n => n.ToString("f3"))) }\r\n";
                text += $"Cf={String.Join(",", control.Cos.Select(n => n.ToString("f3")))}\r\n";
                text += $"Co={String.Join(",", control.Cfs.Select(n => n.ToString("f3")))}\r\n";
            }
            File.WriteAllText("voice.wconfig", text, Encoding.UTF8);
        }

        void Open()
        {
            
        }

        void Generate()
        {
            string text = "";
            foreach (WavControl control in WavControls)
            {
                text += control.Generate();
            }
            File.WriteAllText("oto.ini", text, Encoding.UTF8);
        }

        private void MenuSave_Click(object sender, RoutedEventArgs e)
        {
            Save();
        }

        private void MenuOpen_Click(object sender, RoutedEventArgs e)
        {
            Open();
        }

        private void MenuGenerate_Click(object sender, RoutedEventArgs e)
        {
            Generate();
        }
    }
}
