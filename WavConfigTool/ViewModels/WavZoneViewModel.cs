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
        public PhonemeType Type { get; set; } = PhonemeType.Consonant;
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];

        public WavZoneViewModel() { }

        public WavZoneViewModel(PhonemeType type, double p_in, double p_out)
        {
            In = p_in;
            Out = p_out - p_in;
            Type = type;
            // TODO: Переделать на StyleSelector
            switch (type)
            {
                case PhonemeType.Vowel:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["VowelZoneBrush"];
                    break;

                case PhonemeType.Consonant:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
                    break;

                case PhonemeType.Rest:
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
