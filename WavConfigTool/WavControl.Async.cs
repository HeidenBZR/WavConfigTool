using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace WavConfigTool
{
    public partial class WavControl
    {


        public async void Init()
        {
            IsToDraw = true;
            await GenerateWaveformAsync(true);
        }

        public async void Draw()
        {
            try
            {
                if (!IsEnabled || !IsToDraw)
                    return;
                int i = 0;
                while (!IsImageGenerated && i < WaitingLimit)
                {
                    await Task.Run(() => { Thread.Sleep(100); });
                    i += 100;
                }
                if (!IsImageGenerated)
                    throw new Exception("Waveform generation timeout exceeded");
                bool hasImage = false;
                Dispatcher.Invoke(delegate { hasImage = WavImage.Source != null; });
                if (!hasImage)
                    await OpenImageAsync();
                Dispatcher.Invoke(delegate
                {
                    Height = 100;
                    Visibility = Visibility.Visible;
                    DrawConfig();
                    SetLoaded();
                });
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "error on Draw");
            }
        }


        public async Task<bool> GenerateWaveformAsync(bool force)
        {
            if (!IsEnabled)
                return false;
            try
            {

                if (await Task.Run<bool>(delegate ()
                {
                    try
                    {
                        Dispatcher.Invoke(() => { SetUnloaded(); });
                        if (Thread != null && Thread.IsAlive)
                            Thread.Join();
                        Thread = Thread.CurrentThread;
                        if (!GenerateWaveform(force))
                            return false;
                        return true;
                    }
                    catch (ThreadAbortException) { return false; }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on GenerateWaveformsAsync",
                            MessageBoxButton.OK, MessageBoxImage.Error);
                        return false;
                    }
                })) ;
            }
            catch (ThreadAbortException) { return false; }
            if (await OpenImageAsync())
                    return true;
            return false;
        }


        public async Task<bool> OpenImageAsync()
        {
            if (!IsEnabled)
                return false;

            int i = 0;

            return await Task.Run(() =>
            {
                try
                {
                    //while (Thread == null ) { }
                    //while (Thread.IsAlive) { }
                    //Thread = Thread.CurrentThread;
                    if (!IsImageGenerated && !IsGenerating)
                    {
                        //Dispatcher.Invoke(() => { SetLoaded(); });
                        Undraw();
                        return false;
                    }
                    while(IsGenerating)
                    { }

                    Dispatcher.Invoke(() => {
                        if (OpenImage())
                            SetLoaded();
                    }); 
                    return true;
                }
                catch (ThreadAbortException) { return false; }
                catch (Exception ex)
                {
                    MainWindow.MessageBoxError(ex, "Error on Open Image Async");
                    return false;
                }
            });
        }


        public bool GenerateWaveform(bool force = false)
        {
            try
            {

                if (!File.Exists(Recline.Path))
                    return false;
                if (!force && File.Exists(ImagePath))
                    return true;
                while (IsGenerating)
                {
                    // Wait for previous generating end;
                    Thread.Sleep(10);
                }
                IsGenerating = true;
                WaveForm = new WaveForm(Recline.Path);
                var points = WaveForm.GetAudioPoints();
                if (Ds == null || Ds.Count == 0)
                    Ds = new List<double>();
                //Ds = wave.Ds;
                MostLeft = WaveForm.MostLeft;

                int width = (int)points.Last().X;

                if (!WaveForm.IsEnabled)
                {
                    IsGenerating = false;
                    return false;
                }
                Length = (int)(WaveForm.Length * 1000 / WaveForm.SampleRate);
                WaveForm.PointsToImage((object)(points, width, 100, this));
                IsGenerating = false;
                if (!WaveForm.IsGenerated)
                {
                    MessageBox.Show($"{WaveForm.GeneratingException.Message}\r\n\r\n{WaveForm.GeneratingException.StackTrace}",
                        "Error on image generation", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
                return true;
            }
            catch (ThreadAbortException) { return false; }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on Generate Waveform");
            }
            return false;
        }

        public bool OpenImage()
        {
            try
            {
                BitmapImage src = new BitmapImage();
                src.BeginInit();
                src.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                src.UriSource = new Uri(ImagePath, UriKind.Relative);
                src.CacheOption = BitmapCacheOption.OnLoad;
                src.EndInit();
                src.Freeze();
                WavImage.Source = src;
                Width = Length * ScaleX;
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on OpenImage");
                return false;
            }
        }

    }
}
