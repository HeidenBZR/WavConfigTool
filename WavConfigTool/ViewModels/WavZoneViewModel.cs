using DevExpress.Mvvm;
using System;
using System.Windows;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    public class WavZoneViewModel : ViewModelBase
    {
        public double In { get; set; } = 0;
        public double Out { get; set; } = 100;
        public PhonemeType Type { get; set; } = PhonemeType.Consonant;
        public double Attack => ProjectManager.Current.Project.AttackOfType(Type);

        public double Width => Out + Attack;
        public double Decay => Type == PhonemeType.Vowel ? ProjectManager.Current.Project.VowelDecay : Attack;

        public PointCollection Points { get; private set; }
        public PointCollection BorderPoints1 { get; private set; }
        public PointCollection BorderPoints2 { get; private set; }
        public PointCollection BorderPoints3 { get; private set; }
        public Brush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
        public Brush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];

        public WavZoneViewModel() { }

        public string Name => $"Zone [{string.Join(", ", Points)}]";

        public int Height => WavControlBaseViewModel.GlobalHeight;
        public int Middle => Height / 2;

        public WavZoneViewModel(PhonemeType type, double p_in, double p_out, double length)
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
            double attack = Settings.RealToViewX(Attack);
            double outMinusAttack = Out - attack;
            double inMinusAttack = Math.Max(In - attack, 0);
            double outPlusAttack = Math.Min(Out + attack, length);
            outMinusAttack = outMinusAttack < 0 ? 0 : outMinusAttack;
            double decay = Settings.RealToViewX(ProjectManager.Current.Project.VowelDecay);
            switch (Type)
            {
                case PhonemeType.Consonant:
                    if (outMinusAttack > 0)
                    {
                        BorderPoints1 = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(0, Middle),
                            new Point(outMinusAttack, Middle),
                            new Point(outMinusAttack, Height),
                        };
                    }
                    BorderPoints2 = new PointCollection
                        {
                            new Point(outMinusAttack, Height),
                            new Point(outMinusAttack, Middle),
                            new Point(Out, Height),
                        };
                    Points = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(0, Middle),
                            new Point(outMinusAttack, Middle),
                            new Point(Out, Height),
                        };
                    break;
                case PhonemeType.Vowel:
                    var minLength = 10;
                    var totalLength = Out;
                    var processedAttack = attack;
                    var totalLengthMinusAttack = totalLength - processedAttack;
                    var processedDecay = totalLengthMinusAttack - decay > minLength ? decay : totalLengthMinusAttack - minLength;
                    BorderPoints1 = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(0, 0),
                            new Point(processedDecay, Middle),
                            new Point(processedDecay, Height),
                        };
                    BorderPoints2 = new PointCollection
                        {
                            new Point(processedDecay, Middle),
                            new Point(totalLengthMinusAttack, Middle),
                            new Point(totalLengthMinusAttack, Height),
                            new Point(processedDecay, Height),
                        };
                    BorderPoints3 = new PointCollection
                        {
                            new Point(totalLengthMinusAttack, Middle),
                            new Point(Out, Height),
                            new Point(totalLengthMinusAttack, Height),
                        };
                    Points = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(0, 0),
                            new Point(processedDecay, Middle),
                            new Point(totalLengthMinusAttack, Middle),
                            new Point(Out, Height),
                        };
                    break;
                case PhonemeType.Rest:
                    if (p_in == 0)
                    {
                        BorderPoints2 = new PointCollection
                        {
                            new Point(0, Middle),
                            new Point(0, Height),
                            new Point(Out, Height),
                            new Point(Out, Middle),
                        };
                        BorderPoints3 = new PointCollection
                        {
                            new Point(Out, Middle),
                            new Point(outPlusAttack, Height),
                            new Point(Out, Height),
                        };
                        Points = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(0, Middle),
                            new Point(Out, Middle),
                            new Point(outPlusAttack, Height),
                        };
                    }
                    else if (Math.Abs(length - p_out) < 10)
                    {
                        BorderPoints1 = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(decay, Middle),
                            new Point(decay, Height),
                        };
                        BorderPoints2 = new PointCollection
                        {
                            new Point(decay, Middle),
                            new Point(decay, Height),
                            new Point(Out, Height),
                            new Point(Out, Middle),
                        };
                        Points = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(decay, Middle),
                            new Point(Out, Middle),
                            new Point(Out, Height),
                        };
                    }
                    else
                    {
                        BorderPoints1 = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(decay, Middle),
                            new Point(decay, Height),
                        };
                        BorderPoints2 = new PointCollection
                        {
                            new Point(decay, Middle),
                            new Point(decay, Height),
                            new Point(Out, Height),
                            new Point(Out, Middle),
                        };
                        BorderPoints3 = new PointCollection
                        {
                            new Point(Out, Middle),
                            new Point(outPlusAttack, Height),
                            new Point(Out, Height),
                        };
                        Points = new PointCollection
                        {
                            new Point(0, Height),
                            new Point(decay, Middle),
                            new Point(Out, Middle),
                            new Point(outPlusAttack, Height),
                        };
                    }
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
