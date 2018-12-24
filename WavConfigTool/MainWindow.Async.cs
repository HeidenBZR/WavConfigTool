
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    public partial class MainWindow : Window
    {

        void DrawPage(int count)
        {
            try
            {
                foreach (WavControl control in WavControls.GetRange(ItemsOnPage * PageCurrent, count))
                {
                    if (control.IsEnabled)
                    {
                        Dispatcher.Invoke(() => { WaveControlStackPanel.Children.Add(control); });
                        control.Draw();
                    }
                }
                //if (manual)
                //{
                //    TextBoxItemsOnPage.Text = ItemsOnPage.ToString();
                //    TextBoxPage.Text = (PageCurrent + 1).ToString();
                //}
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on draw page");
            }
        }

        async void DrawPageAsync(bool manual = true)
        {
            try
            {
                if (!IsInitialized) return;
                //CanvasLoading.Visibility = Visibility.Visible;
                WaveControlStackPanel.Children.Clear();
                WaveControlStackPanel.Children.Capacity = 0;
                SetPageTotal();
                int count = ItemsOnPage;
                while (ItemsOnPage * PageCurrent + count > WavControls.Count)
                    count--;
                await Task.Run(() => { DrawPage(count); });
                //CanvasLoading.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on Draw Page Async");
            }
        }


    }
}
