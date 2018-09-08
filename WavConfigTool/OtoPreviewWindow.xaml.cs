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
using System.Windows.Shapes;

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для OtoPreviewWindow.xaml
    /// </summary>
    public partial class OtoPreviewWindow : Window
    {
        List<OtoPreviewControl> Controls;

        public OtoPreviewWindow(string filename)
        {
            InitializeComponent();
            Controls = new List<OtoPreviewControl>();
            Title = $"Oto Preview [{filename}]";
        }

        public void Add(OtoPreviewControl control)
        {
            Controls.Add(control);
            OtoPreviewView.Items.Add(control);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            foreach(var control in Controls)
            {
                control.Filename.Margin = new Thickness(e.HorizontalOffset,0,0,0);
            }
        }
    }
}
