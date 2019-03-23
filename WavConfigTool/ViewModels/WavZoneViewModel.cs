using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class WavZoneViewModel : ViewModelBase
    {
        public double In { get; set; } = 0;
        public double Out { get; set; } = 100;
        public WavConfigPoint Type { get; set; } = WavConfigPoint.C;
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];

        public WavZoneViewModel() { }

        public WavZoneViewModel(WavConfigPoint type, double p_in, double p_out)
        {
            In = p_in;
            Out = p_out - p_in;
            Type = type;
            // TODO: Переделать на StyleSelector
            switch (type)
            {
                case WavConfigPoint.V:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["VowelZoneBrush"];
                    break;

                case WavConfigPoint.C:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
                    break;

                case WavConfigPoint.R:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["RestZoneBrush"];
                    break;
            }
            RaisePropertiesChanged(
                () => BackgroundBrush,
                () => In,
                () => Out
            );
        }

    }
}
