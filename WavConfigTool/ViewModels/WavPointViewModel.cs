using DevExpress.Mvvm;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    public class WavPointViewModel : ViewModelBase
    {
        private string text = "Text";

        public string Text { get => text; set { text = value; RaisePropertyChanged(() => VisualText); } }
        public string VisualText => Type == PhonemeType.Rest ? "" : Text;
        public double Position { get; set; } = 0;
        public int PositionReal => Settings.ViewToRealX(Position);
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
        public PhonemeType Type { get; set; } = PhonemeType.Vowel;

        public delegate void WavPointChangedEventHandler(double position1, double position2);
        public delegate void WavPointDeletedEventHandler(double position1);
        public event WavPointChangedEventHandler WavPointChanged = delegate { };
        public event WavPointDeletedEventHandler WavPointDeleted = delegate { };
        public event SimpleHandler RegenerateOtoRequest = delegate { };

        public bool IsLeft { get; set; }
        public bool IsRight => !IsLeft;
        
        public bool HasTopLeftCorner { get; private set; }
        public bool HasTopRightCorner { get; private set; }
        public bool HasBottomLeftCorner { get; private set; }
        public bool HasBottomRightCorner { get; private set; }
        public int IndexZ => IsLeft ? 10 : 1;

        public int Height => WavControlBaseViewModel.GlobalHeight;

        public bool IsLoaded { get; set; } = false;

        public WavPointViewModel()
        {

        }

        public WavPointViewModel(double position, PhonemeType type, string text, bool isLeft)
        {
            Position = position;
            Type = type;
            Update(isLeft, text);
            // TODO: Переделать на StyleSelector
            switch (Type)
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

        public void Update(bool isLeft, string text)
        {
            IsLeft = isLeft;
            Text = text;
            // TODO: Переделать на StyleSelector
            switch (Type)
            {
                case PhonemeType.Vowel:
                case PhonemeType.Consonant:
                    HasTopLeftCorner = IsRight;
                    HasTopRightCorner = false;
                    HasBottomRightCorner = false;
                    HasBottomLeftCorner = false;
                    break;

                case PhonemeType.Rest:
                    HasTopLeftCorner = IsRight;
                    HasTopRightCorner = IsLeft;
                    HasBottomRightCorner = IsLeft;
                    HasBottomLeftCorner = IsRight;
                    break;
            }
            RaisePropertiesChanged(
                () => HasTopRightCorner,
                () => HasTopLeftCorner,
                () => HasBottomLeftCorner,
                () => HasBottomRightCorner
            );
            RaisePropertiesChanged(
                () => IsLeft,
                () => IsRight,
                () => Text
            );
        }

        public override string ToString()
        {
            return $"{{{Position}}} {Text}";
        }

        #region commands

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

        #endregion
    }
}
