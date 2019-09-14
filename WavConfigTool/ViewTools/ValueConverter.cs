using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewTools
{
    public class PhonemeTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (PhonemeType)value;
            if (type == PhonemeType.Consonant)
                return (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
            else if (type == PhonemeType.Vowel)
                return (SolidColorBrush)Application.Current.Resources["VowelBackBrush"];
            else if (type == PhonemeType.Rest)
                return (SolidColorBrush)Application.Current.Resources["RestBackBrush"];
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    public class PhonemeTypeToBorderColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var type = (PhonemeType)value;
            if (type == PhonemeType.Consonant)
                return (SolidColorBrush)Application.Current.Resources["ConsonantBorderBrush"];
            else if (type == PhonemeType.Vowel)
                return (SolidColorBrush)Application.Current.Resources["VowelBorderBrush"];
            else if (type == PhonemeType.Rest)
                return (SolidColorBrush)Application.Current.Resources["RestBorderBrush"];
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class BooleanToFontWeightConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var val = (bool)value;
            return val ? FontWeights.Bold : FontWeights.Normal;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
