using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using System.Windows.Media;
using WavConfigTool.Classes;
using WavConfigCore;
using Brush = System.Windows.Media.Brush;
using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Pen = System.Drawing.Pen;
using System.Threading.Tasks;

namespace WavConfigTool.ViewModels
{
    public class WavZoneViewModel : ViewModelBase
    {
        public double Position => In;
        public double In { get; set; } = 0;
        public double Out { get; set; } = 100;
        public PhonemeType Type { get; set; } = PhonemeType.Consonant;
        public double Attack => ProjectManager.Current.Project.AttackOfType(Type);

        public double Width => Out + Attack;
        public double Decay => ProjectManager.Current.Project.DecayOfType(Type);

        public ImageSource Image { get; private set; }
        public SolidColorBrush BackgroundBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantZoneBrush"];
        public SolidColorBrush BorderBrush { get; set; } = (SolidColorBrush)Application.Current.Resources["ConsonantBackBrush"];

        public WavZoneViewModel() { }

        public int Height { get; private set; }
        public int Middle => Height / 2;

        public WavZoneViewModel(PhonemeType type, double p_in, double p_out, double length, int height)
        {
            In = p_in;
            Out = p_out - p_in;
            Type = type;
            Height = height;
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
            double decay = Settings.RealToViewX(Decay);
            double[][] lines = null;
            double[] points = null;
            double width = Out;
            switch (Type)
            {
                case PhonemeType.Consonant:
                    if (outMinusAttack > 0)
                    {
                        lines = new double[][]
                        {
                            new double[]
                            {
                                outMinusAttack, Height,
                                outMinusAttack, Middle
                            },
                        };
                    }
                    else
                    {
                        lines = new double[0][];
                    }
                    points = new double[]
                    {
                        0, Height,
                        Out, Height,
                        Out, Middle,
                        0, Middle,
                    };
                    break;
                case PhonemeType.Vowel:
                    var minLength = 10;
                    var totalLength = Out;
                    var processedAttack = attack;
                    var totalLengthMinusAttack = totalLength - processedAttack;
                    var processedDecay = totalLengthMinusAttack - decay > minLength ? decay : totalLengthMinusAttack - minLength;
                    lines = new[]
                    {
                        new double[]
                        {
                            processedDecay, Height,
                            processedDecay, Middle
                        },
                        new double[]
                        {
                            totalLengthMinusAttack, Height,
                            totalLengthMinusAttack, Middle
                        }
                    };
                    points = new double[]
                    {
                        0, Height,
                        0, 0,
                        processedDecay, Middle,
                        totalLengthMinusAttack, Middle,
                        Out, Height
                    };
                    break;
                case PhonemeType.Rest:
                    if (p_in == 0)
                    {
                        lines = new double[][]
                        {
                            new double[]
                            {
                                Out, Middle,
                                Out, Height
                            }
                        };
                        points = new double[]
                        {
                            0, Height,
                            0, Middle,
                            Out, Middle,
                            outPlusAttack, Height,
                        };
                        width = Out + outPlusAttack;
                    }
                    else if (Math.Abs(length - p_out) < 10)
                    {
                        if (decay > Out)
                        {
                            lines = new double[][]
                            {
                                new double[]
                                {
                                    0, Middle,
                                    0, Height
                                }
                            };
                            points = new double[]
                            {
                                0, Height,
                                Out, Middle,
                                Out, Height
                            };
                        }
                        else
                        {
                            lines = new double[][]
                            {
                                new double[]
                                {
                                    Out, Middle,
                                    Out, Height
                                }
                            };
                            points = new double[]
                            {
                                0, Height,
                                0, Middle,
                                Out, Middle,
                                outPlusAttack, Height,
                            };
                        }
                    }
                    else
                    {
                        lines = new double[][]
                        {
                            new double[]
                            {
                                decay, Middle,
                                decay, Height
                            },
                            new double[]
                            {
                                Out, Middle,
                                Out, Height
                            }
                        };
                        points = new double[]
                        {
                            0, Height,
                            decay, Middle,
                            Out, Middle,
                            outPlusAttack, Height,
                        };
                    }
                    break;
            }

            var fillBrush = Color.FromArgb(BackgroundBrush.Color.A,BackgroundBrush.Color.R, BackgroundBrush.Color.G, BackgroundBrush.Color.B);
            var strokeBrush = Color.FromArgb(BorderBrush.Color.R, BorderBrush.Color.G, BorderBrush.Color.B);
            DrawPointsAsync(lines, points, (int)width, Height, fillBrush, strokeBrush);
        }

        private async void DrawPointsAsync(double[][] lines, double[] points, int width, int height, Color fillColor, Color strokeColor)
        {
            await Task.Run(() =>
            {
                DrawPoints(lines, points, width, height, fillColor, strokeColor);
            }).ContinueWith(delegate
            {
                RaisePropertyChanged(nameof(Image));
            });
        }

        private void DrawPoints(double[][] lines, double[] points, int width, int height, Color fillColor, Color strokeColor)
        {
            if (width == 0)
                return;
            var res = new Bitmap(width, height);
            using (var strokePen = new Pen(strokeColor))
            using (var fillBrush = new SolidBrush(fillColor))
            using (Graphics g = Graphics.FromImage(res))
            {
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                foreach (var line in lines)
                {
                    g.DrawLine(strokePen, (float)line[0], (float)line[1], (float)line[2], (float)line[3]);
                }

                var pointFs = new List<PointF>();
                for (int i = 0; i + 1 < points.Length; i += 2)
                {
                    var x1 = (float)points[i];
                    var y1 = (float)points[i + 1];
                    var x2 = (float)points[i + 2 >= points.Length ? 0 : i + 2];
                    var y2 = (float)points[i + 3 >= points.Length ? 1 : i + 3];
                    pointFs.Add(new PointF(x1, y1));
                    if (x1 == x2 && (x1 == In || x1 == width))
                        continue;
                    g.DrawLine(strokePen, x1, y1, x2, y2);
                }
                pointFs.Add(new PointF((float)points[0], (float)points[1]));

                g.FillPolygon(fillBrush, pointFs.ToArray());
            }

            var bitmapImage = WaveForm.Bitmap2BitmapImage(res);
            res.Dispose();
            Image = bitmapImage;
        }
    }
}
