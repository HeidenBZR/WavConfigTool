using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using DevExpress.Mvvm;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class WavPointViewModel : ViewModelBase
    {
        public string Text { get; set; } = "Text";
        public double Position { get; set; } = 0;
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
        public WavConfigPoint Type { get; set; } = WavConfigPoint.V;

        public WavPointViewModel()
        {
             
        }

        public WavPointViewModel(double position, WavConfigPoint type, string text)
        {
            Position = position;
            Type = type;
            Text = text;
            // TODO: Переделать на StyleSelector
            switch (type)
            {
                case WavConfigPoint.V:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["VowelBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["VowelBackBrush"];
                    break;

                case WavConfigPoint.C:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
                    break;

                case WavConfigPoint.R:
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["RestBorderBrush"];
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["RestBackBrush"];
                    break;
            }
            RaisePropertiesChanged(
                () => BorderBrush,
                () => BackgroundBrush
            );
        }

        public ICommand DragDeltaCommand
        {
            get
            {
                return new DelegateCommand((x) => 
                {

                }, param => param != null);
            }
        }
    }
}
