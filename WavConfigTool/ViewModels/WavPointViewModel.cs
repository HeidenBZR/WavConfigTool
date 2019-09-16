using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class WavPointViewModel : ViewModelBase
    {
        private string text = "Text";

        public string Text { get => text; set { text = value; FireChanged(); } }
        public string VisualText => Type == PhonemeType.Rest ? "" : Text;
        public double Position { get; set; } = 0;
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
        public PhonemeType Type { get; set; } = PhonemeType.Vowel;

        public delegate void WavPointChangedEventHandler(double position1, double position2);
        public delegate void WavPointDeletedEventHandler(double position1);
        public delegate void SimpleHandler();
        public event WavPointChangedEventHandler WavPointChanged = delegate { };
        public event WavPointDeletedEventHandler WavPointDeleted = delegate { };
        public event SimpleHandler RegenerateOtoRequest = delegate { };

        public bool IsLoaded { get; set; } = false;

        public WavPointViewModel()
        {

        }

        public WavPointViewModel(double position, PhonemeType type, string text)
        {
            Position = position;
            Type = type;
            Text = text;
            // TODO: Переделать на StyleSelector
            switch (type)
            {
                case PhonemeType.Vowel:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["VowelBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["VowelBackBrush"];
                    break;

                case PhonemeType.Consonant:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
                    break;

                case PhonemeType.Rest:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["RestBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["RestBackBrush"];
                    break;
            }
            RaisePropertiesChanged(
                () => BorderBrush,
                () => BackgroundBrush
            );
        }

        public ICommand PointMovedCommand
        {
            get
            {
                return new DelegateCommand<Point>(
                    delegate (Point point)
                    {
                        var oldValue = Position;
                        Position = point.X;
                        WavPointChanged(oldValue, Position);
                    },
                    delegate (Point point)
                    {
                        return true;
                    }
                );
            }
        }

        public ICommand DeletePointCommand => new DelegateCommand(
            () => WavPointDeleted(Position),
            () => true
        );
        public ICommand RegenerateOtoRequestCommand => new DelegateCommand(
            () => RegenerateOtoRequest(),
            () => true
        );

        public override string ToString()
        {
            return $"{{{Position}}} {Text}";
        }

        public void FireChanged()
        {
            RaisePropertyChanged("VisualText");
        }
    }
}
