using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigTool.Tools;

namespace WavConfigTool.ViewModels
{
    public class WavZoneViewModel : ViewModelBase
    {
        public double In { get; set; } = 0;
        public double Out { get; set; } = 100;
        public PhonemeType Type { get; set; } = PhonemeType.Consonant;
        public double Attack => Type == PhonemeType.Consonant ? Project.Current.ConsonantAttack :
                                (Type == PhonemeType.Vowel ? Project.Current.VowelAttack : Project.Current.RestAttack);

        public double Width => Out + Attack;
        public double Decay => Type == PhonemeType.Vowel ? Project.Current.VowelDecay : Attack;

        public PointCollection Points { get; private set; }
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];

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
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["VowelBackBrush"];
                    break;

                case PhonemeType.Consonant:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];
                    break;

                case PhonemeType.Rest:
                    BackgroundBrush = (SolidColorBrush)Application.Current.Resources["RestZoneBrush"];
                    BorderBrush = (SolidColorBrush)Application.Current.Resources["RestBackBrush"];
                    break;
            }
            switch (Type)
            {
                case PhonemeType.Consonant:
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(-Settings.RealToViewX(Attack), 100),
                            new Point(0, 0),
                            new Point(Out, 0),
                            new Point(Out, 100),
                        };
                    break;
                case PhonemeType.Vowel:
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(0, 0),
                            new Point(Settings.RealToViewX(Decay), 50),
                            new Point(Out - Settings.RealToViewX(Attack), 50),
                            new Point(Out, 100),
                            new Point(Settings.RealToViewX(Decay), 100),
                            new Point(Settings.RealToViewX(Decay), 50),
                            new Point(Out - Settings.RealToViewX(Attack), 50),
                            new Point(Out - Settings.RealToViewX(Attack), 100),
                        };
                    break;
                case PhonemeType.Rest:
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(Settings.RealToViewX(Project.Current.VowelDecay), 50),
                            new Point(0, 50),
                            new Point(Out - Settings.RealToViewX(Attack), 50),
                            new Point(Out, 100),
                        };
                    break;
            }
            RaisePropertiesChanged(
                () => BackgroundBrush,
                () => In,
                () => Out,
                () => Points
            );
        }

    }
}
