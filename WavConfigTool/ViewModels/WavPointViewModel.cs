using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class WavPointViewModel : ViewModelBase
    {
        public string Text { get; set; } = "Text";
        public double Position { get; set; } = 0;
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
        public PhonemeType Type { get; set; } = PhonemeType.Vowel;

        public delegate void WavPointChangedEventHandler(double position1, double position2);
        public delegate void WavPointDeletedEventHandler(double position1);
        public event WavPointChangedEventHandler WavPointChanged;
        public event WavPointDeletedEventHandler WavPointDeleted;

        public bool IsLoaded { get; set; } = false;

        public WavPointViewModel()
        {
            WavPointChanged += delegate { };
            WavPointDeleted += delegate { };
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

        public ICommand DeletePointCommand
        {
            get
            {
                return new DelegateCommand(
                    () => WavPointDeleted(Position),
                    () => true
                );
            }
        }

        public override string ToString()
        {
            return $"{{{Position}}} {Text}";
        }
    }
}
