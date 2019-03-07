using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для OtoPreviewControl.xaml
    /// </summary>
    public partial class OtoPreviewControl : UserControl
    {
        public static double ScaleX { get { return WavControl.ScaleX; } }
        public double Left;

        public OtoPreviewControl(ImageSource source, string description, int[] ops, int length)
        {
            InitializeComponent(); 
            try
            {
                ImageWav.Source = source;
                Filename.Content = description;
                Offset.Width = ops[0] * ScaleX;
                Consonant.Margin = new Thickness(Offset.Width, 0, 0, 0);
                Consonant.Width = ops[1] * ScaleX;
                if (ops[2] < 0)
                {
                    Cutoff.Margin = new Thickness(Offset.Width - ops[2] * ScaleX, 0, 0, 0);
                    Cutoff.Width = length * ScaleX / 4 - Offset.Width + ops[2] * ScaleX;
                }
                else
                {
                    Cutoff.Margin = new Thickness(0, 0, 0, 0);
                    Cutoff.Width = length * ScaleX / 4;
                    HorizontalAlignment = HorizontalAlignment.Right;
                }
                if (ops[2] >= 0) Cutoff.Width = ops[2] * ScaleX;
                Preutterance.Margin = new Thickness(Offset.Width + ops[3] * ScaleX, 0, 0, 0);
                Overlap.Margin = new Thickness(Offset.Width + ops[4] * ScaleX, 0, 0, 0);
                Left = Offset.Width;
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, $"Error on drawing oto preview for {description}");
            }
        }
    }
}
