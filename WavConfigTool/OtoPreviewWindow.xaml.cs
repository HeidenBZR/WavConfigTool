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
            Loaded += delegate
            {
                InitScroll();
            };
        }

        public void Add(OtoPreviewControl control)
        {
            Controls.Add(control);
            OtoPreviewView.Items.Add(control);
        }

        void InitScroll()
        {
            var offset = ScrollViewer.ScrollableWidth / 2 - ScrollViewer.ActualWidth / 2;
            if (offset < 0) offset = 0;
            ScrollViewer.ScrollToHorizontalOffset(offset);
            ScrollContent(offset);
        }

        void ScrollContent()
        {
            foreach (var control in Controls)
                control.Filename.Margin = new Thickness(ScrollViewer.HorizontalOffset, 0, 0, 0);
        }
        void ScrollContent(double offset)
        {
            foreach (var control in Controls)
                control.Filename.Margin = new Thickness(offset, 0, 0, 0);
        }


        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollContent();
        }


    }
}
