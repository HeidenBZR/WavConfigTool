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
        public event WavMarkerMoveEventHandler WavMarkerDelete;
        public event WavMarkerMoveEventHandler WavMarkerCreated;
        bool IsClosed = false;

        public WavMarker(Phoneme phoneme, double x, int i)
        {
            InitializeComponent();
            Position = x;
            Margin = new Thickness(x * WavControl.ScaleX, 0, 0, 0);
            Phoneme = phoneme;
            WavMarkerCreated = delegate { };
            if (i == 0)
            {
                phoneme.Zone.In = this;
                TypeLabel.Content = Phoneme.Alias;
                Panel.SetZIndex(this, 0);
            }
            else
            {
                IsClosed = true;
                phoneme.Zone.Out = this;
                TypeLabel.Visibility = Visibility.Hidden;
                Line.HorizontalAlignment = HorizontalAlignment.Right;
                MarkerController.HorizontalAlignment = HorizontalAlignment.Right;
                MarkerController.Style = FindResource("WavMarkerControllerStyleClosed") as Style;
                Margin = new Thickness(Margin.Left - Width, Margin.Top, Margin.Right, Margin.Bottom);
                Panel.SetZIndex(this, 10);
            }
        }

        public WavMarker(Vowel phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["VBackBrush"];
            Resources["BorderBrush"] = Resources["VBorderBrush"];
            Type = WavConfigPoint.V;
            Height = 100;
            Grid.Margin = new Thickness(0, 10, 0, 0);
            VerticalAlignment = VerticalAlignment.Bottom;
            MarkerController.VerticalAlignment = VerticalAlignment.Top;
            TypeLabel.VerticalAlignment = VerticalAlignment.Top;
            WavMarkerCreated(Position);
        }
        public WavMarker(Consonant phoneme, double x, int i) : this(phoneme as Phoneme, x, i)
        {
            Resources["BackBrush"] = Resources["CBackBrush"];
            Resources["BorderBrush"] = Resources["CBorderBrush"];
            Height = 100;
            Grid.Margin = new Thickness(0, 0, 0, 10);
            Type = WavConfigPoint.C;
            VerticalAlignment = VerticalAlignment.Top;
            MarkerController.VerticalAlignment = VerticalAlignment.Bottom;
            TypeLabel.VerticalAlignment = VerticalAlignment.Bottom;
            WavMarkerCreated(Position);
        }
        public WavMarker(double x, int i) : this(new Rest("-"), x, i)
        {
            Resources["BackBrush"] = Resources["DBackBrush"];
            Resources["BorderBrush"] = Resources["DBorderBrush"];
            Height = 70;
            Type = WavConfigPoint.D;
            TypeLabel.Content = "";
            VerticalAlignment = VerticalAlignment.Top;
            MarkerController.VerticalAlignment = VerticalAlignment.Bottom;
            TypeLabel.VerticalAlignment = VerticalAlignment.Bottom;
            WavMarkerCreated(Position);
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MarkerController_DragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            Position += e.HorizontalChange / WavControl.ScaleX;
            Margin = new Thickness(Position * WavControl.ScaleX, 0, 0, 0);
            if (IsClosed)
                Margin = new Thickness(Margin.Left - Width, Margin.Top, Margin.Right, Margin.Bottom);
        }

        private void MarkerController_DragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            WavMarkerMoved(Position);
        }

        private void MarkerController_DragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
                WavMarkerDelete(Position);
        }
    }
}
