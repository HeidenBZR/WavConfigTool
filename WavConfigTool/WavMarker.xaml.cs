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

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для WavMarker.xaml
    /// </summary>
    public partial class WavMarker : UserControl
    {
        public Phoneme Phoneme;
        public WavConfigPoint Type;
        public double Position;
        public double Msec { get { return Position * 1000; } }

        public WavMarker() { InitializeComponent(); }

        public WavMarker(Vowel vowel, double x, int i) : this()
        {
            Resources["BackBrush"] = Resources["VBackBrush"];
            Resources["BorderBrush"] = Resources["VBorderBrush"];
            Position = x;
            Height = 100;
            Type = WavConfigPoint.V;
            Phoneme = vowel;
            if (i == 0) vowel.Marker = this;
            else vowel.MarkerOut = this;
        }
        public WavMarker(Occlusive occlusive, double x) : this()
        {
            Resources["BackBrush"] = Resources["CaBackBrush"];
            Resources["BorderBrush"] = Resources["CaBorderBrush"];
            Position = x;
            Height = 90;
            Phoneme = occlusive;
            Type = WavConfigPoint.Co;
            occlusive.Marker = this;
        }
        public WavMarker(Frivative frivative, double x, int i) : this()
        {
            Resources["BackBrush"] = Resources["CfBackBrush"];
            Resources["BorderBrush"] = Resources["CfBorderBrush"];
            Position = x;
            Height = 90;
            Phoneme = frivative;
            Type = WavConfigPoint.Cf;
            if (i == 0) frivative.Marker = this;
            else frivative.MarkerOut = this;
        }
        public WavMarker(double x) : this()
        {
            Resources["BackBrush"] = Resources["DBackBrush"];
            Resources["BorderBrush"] = Resources["DBorderBrush"];
            Position = x;
            Height = 15;
            Type = WavConfigPoint.D;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            Margin = new Thickness(Position * WavControl.ScaleX, 0, 0, 0);
            switch (Type)
            {
                case WavConfigPoint.V:
                    MarkerController.Margin = new Thickness(0, 20, 0, 0);
                    Line.Margin = new Thickness(0, 20, 0, 0);
                    TypeLabel.Margin = new Thickness(17, 20, 0, 0);
                    TypeLabel.Content = Phoneme.Alias;
                    break;
                case WavConfigPoint.Co:
                    MarkerController.Margin = new Thickness(0, 75, 0, 0);
                    TypeLabel.Margin = new Thickness(0, 75, 0, 0);
                    TypeLabel.Content = Phoneme.Alias;
                    break;
                case WavConfigPoint.Cf:
                    MarkerController.Margin = new Thickness(0, 75, 0, 0);
                    TypeLabel.Margin = new Thickness(17, 75, 0, 0);
                    TypeLabel.Content = Phoneme.Alias;
                    break;
                case WavConfigPoint.D:
                    MarkerController.Margin = new Thickness(0, 0, 0, 0);
                    TypeLabel.Margin = new Thickness(17, 0, 0, 0);
                    TypeLabel.Content = "D";
                    break;
            }
        }
    }
}
