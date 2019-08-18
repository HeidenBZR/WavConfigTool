using DevExpress.Mvvm;
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
        public PointCollection BorderPoints1 { get; private set; }
        public PointCollection BorderPoints2 { get; private set; }
        public PointCollection BorderPoints3 { get; private set; }
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
            double outMinusAttack = Out - Settings.RealToViewX(Attack);
            outMinusAttack = outMinusAttack < 0 ? 0 : outMinusAttack;
            double decay = Settings.RealToViewX(Project.Current.VowelDecay);
            switch (Type)
            {
                case PhonemeType.Consonant:
                    BorderPoints1 = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(0, 50),
                            new Point(outMinusAttack, 50),
                            new Point(outMinusAttack, 100),
                        };
                    BorderPoints2 = new PointCollection
                        {
                            new Point(outMinusAttack, 100),
                            new Point(outMinusAttack, 50),
                            new Point(Out, 100),
                        };
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(0, 50),
                            new Point(outMinusAttack, 50),
                            new Point(Out, 100),
                        };
                    break;
                case PhonemeType.Vowel:
                    BorderPoints1 = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(0, 0),
                            new Point(decay, 50),
                            new Point(decay, 100),
                        };
                    BorderPoints2 = new PointCollection
                        {
                            new Point(decay, 50),
                            new Point(outMinusAttack, 50),
                            new Point(outMinusAttack, 100),
                            new Point(decay, 100),
                        };
                    BorderPoints3 = new PointCollection
                        {
                            new Point(outMinusAttack, 50),
                            new Point(Out, 100),
                            new Point(outMinusAttack, 100),
                        };
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(0, 0),
                            new Point(decay, 50),
                            new Point(outMinusAttack, 50),
                            new Point(Out, 100),
                        };
                    break;
                case PhonemeType.Rest:
                    BorderPoints1 = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(decay, 50),
                            new Point(decay, 100),
                        };
                    BorderPoints2 = new PointCollection
                        {
                            new Point(decay, 50),
                            new Point(decay, 100),
                            new Point(outMinusAttack, 100),
                            new Point(outMinusAttack, 50),
                        };
                    BorderPoints3 = new PointCollection
                        {
                            new Point(outMinusAttack, 50),
                            new Point(Out, 100),
                            new Point(outMinusAttack, 100),
                        };
                    Points = new PointCollection
                        {
                            new Point(0, 100),
                            new Point(decay, 50),
                            new Point(outMinusAttack, 50),
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
