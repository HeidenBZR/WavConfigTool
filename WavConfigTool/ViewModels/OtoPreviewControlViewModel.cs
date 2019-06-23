using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.ViewModels
{
    class OtoPreviewControlViewModel
    {

        public OtoPreviewControl(string description, int[] ops, int length)
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
