
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

        void UndrawPage()
        {
            try
            {
                Dispatcher.Invoke(() => { WaveControlStackPanel.Children.Clear(); });
                var min = Settings.ItemsOnPage * Settings.CurrentPage;
                var max = min + Settings.ItemsOnPage;
                var count = WavControls.Count;
                for (int i = min; i < max && i < count; i++)
                    if (WavControls[i].Recline.IsEnabled)
                        WavControls[i].Undraw();
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on draw page");
            }
        }

        void DrawPage()
        {
            try
            {
                var min = Settings.ItemsOnPage * Settings.CurrentPage;
                var max = min + Settings.ItemsOnPage;
                var count = WavControls.Count;
                for (int i = min; i < max && i < count; i++)
                {
                    if (WavControls[i].Recline.IsEnabled)
                    {
                        Dispatcher.Invoke(() => { WaveControlStackPanel.Children.Add(WavControls[i]); });
                        WavControls[i].Draw();
                    }
                }
                //if (manual)
                //{
                //    TextBoxItemsOnPage.Text = Settings.ItemsOnPage.ToString();
                //    TextBoxPage.Text = (Settings.CurrentPage + 1).ToString();
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
                DrawPage();
                //await Task.Run(() => { DrawPage(count); });
                //CanvasLoading.Visibility = Visibility.Hidden;
            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on Draw Page Async");
            }
        }

        async void InitWavcontrols(bool force)
        {
            if (WavControls is null)
                return;
            try
            {
                
                var min = Settings.CurrentPage * Settings.ItemsOnPage;
                var max = (Settings.CurrentPage + 1) * Settings.ItemsOnPage;
                var count = WavControls.Count;


                Parallel.For(min, max, delegate (int i)
                {
                    if (max < count)
                        WavControls[i].Init(true);
                });
                await Task.Run(delegate
                {
                    Parallel.For(0, count, delegate (int i)
                    {
                        if (i < min || i >= max)
                            WavControls[i].Init();
                    });
                });

            }
            catch (Exception ex)
            {
                MessageBoxError(ex, "Error on GenerateWaveformsAsync");
                CanvasLoading.Visibility = Visibility.Hidden;
            }
        }

        async void InitWavcontrolsAsync(bool force = true)
        {
            //Dispatcher.Invoke(() => { CanvasLoading.Visibility = Visibility.Visible; });
            //await Task.Run(() =>
            //{
            //    //if (Thread != null && Thread.IsAlive)
            //    //    Thread.Join();
            //    //Dispatcher.Invoke(() => { CanvasLoading.Visibility = Visibility.Hidden; });
            //    Thread = Thread.CurrentThread;
            //    GenerateWaveforms(force);
            //});
            InitWavcontrols(force);
        }


    }
}
