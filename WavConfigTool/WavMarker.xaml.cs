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
        public delegate void WavMarkerMoveEventHandler(double x);
        public event WavMarkerMoveEventHandler WavMarkerMoved;

        public WavMarker(Phoneme phoneme, double x, int i)
        {
            InitializeComponent();
            Position = x;
            Margin = new Thickness(x * WavControl.ScaleX, 0, 0, 0);
            Phoneme = phoneme;
            if (i == 0) phoneme.Zone.In = this;
            else phoneme.Zone.Out = this;
            TypeLabel.Content = Phoneme.Alias;
        }

        public WavMarker(Vowel phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["VBackBrush"];
            Resources["BorderBrush"] = Resources["VBorderBrush"];
            Type = WavConfigPoint.V;
            Grid.Margin = new Thickness(0, 30, 0, 0);
            VerticalAlignment = VerticalAlignment.Bottom;
            MarkerController.VerticalAlignment = VerticalAlignment.Top;
            TypeLabel.VerticalAlignment = VerticalAlignment.Top;
        }
        public WavMarker(Consonant phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["CBackBrush"];
            Resources["BorderBrush"] = Resources["CBorderBrush"];
            Height = 90;
            Type = WavConfigPoint.C;
            VerticalAlignment = VerticalAlignment.Top;
            MarkerController.VerticalAlignment = VerticalAlignment.Bottom;
            TypeLabel.VerticalAlignment = VerticalAlignment.Bottom;
        }
        public WavMarker(double x, int i) : this(new Rest("-"), x, i)
        {
            Resources["BackBrush"] = Resources["DBackBrush"];
            Resources["BorderBrush"] = Resources["DBorderBrush"];
            Height = 30;
            Type = WavConfigPoint.D;
            TypeLabel.Content = "D";
            VerticalAlignment = VerticalAlignment.Top;
            MarkerController.VerticalAlignment = VerticalAlignment.Bottom;
            TypeLabel.VerticalAlignment = VerticalAlignment.Bottom;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MarkerController_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Position += e.HorizontalChange / WavControl.ScaleX;
            Margin = new Thickness(Position * WavControl.ScaleX, 0, 0, 0);
        }

        private void MarkerController_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            WavMarkerMoved(Position);
        }
    }
}
