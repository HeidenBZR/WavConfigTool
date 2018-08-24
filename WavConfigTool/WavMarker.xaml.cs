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

        public WavMarker(Phoneme phoneme, double x, int i)
        {
            InitializeComponent();
            Position = x;
            Phoneme = phoneme;
            if (i == 0) phoneme.Zone.In = this;
            else phoneme.Zone.Out = this;
            TypeLabel.Content = Phoneme.Alias;
        }

        public WavMarker(Vowel phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["VBackBrush"];
            Resources["BorderBrush"] = Resources["VBorderBrush"];
            Height = 100;
            Type = WavConfigPoint.V;
            MarkerController.Margin = new Thickness(0, 20, 0, 0);
            Line.Margin = new Thickness(0, 20, 0, 0);
            TypeLabel.Margin = new Thickness(17, 20, 0, 0);
        }
        public WavMarker(Consonant phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["CBackBrush"];
            Resources["BorderBrush"] = Resources["CBorderBrush"];
            MarkerController.Margin = new Thickness(0, 75, 0, 0);
            TypeLabel.Margin = new Thickness(17, 75, 0, 0);
            Height = 90;
            Type = WavConfigPoint.C;
        }
        public WavMarker(double x, int i) : this(new Rest("-"), x, i)
        {
            Resources["BackBrush"] = Resources["DBackBrush"];
            Resources["BorderBrush"] = Resources["DBorderBrush"];
            Height = 15;
            Type = WavConfigPoint.D;
            MarkerController.Margin = new Thickness(0, 0, 0, 0);
            TypeLabel.Margin = new Thickness(17, 0, 0, 0);
            TypeLabel.Content = "D";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
