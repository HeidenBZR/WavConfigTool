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
        public delegate void ImageLoadedHandler();
        public ImageLoadedHandler OnImageLoaded;

        public async void Init(bool draw = false)
        {
            IsToDraw = true;
            await Task.Run(delegate { Dispatcher.Invoke(delegate { SetUnloaded(); }); });
            if (InitWave())
            {
                if (await GenerateWaveform_Before())
                    await GenerateWaveformAsync(true);
                if (draw)
                    GenerateWaveform_After();
            }
        }

        public async void Draw()
        {
            try
            {
                //Dispatcher.Invoke(() => {
                //    Visibility = Visibility.Hidden;
                //});
                if (!IsEnabled || !IsToDraw)
                    return;
                int i = 0;
                while ((ImagePath is null || !IsImageGenerated) && i < WaitingLimit)
                {
                    await Task.Run(() => { Thread.Sleep(1000); });
                    i += 1;
                }
                if (!IsImageGenerated)
                    throw new Exception("Waveform generation timeout exceeded");
                bool hasImage = false;
                Dispatcher.Invoke(delegate { hasImage = WavImage.Source != null; });
                if (hasImage || await OpenImageAsync())
                    OnImageLoaded();

            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "error on Draw");
            }
        }

        public Task<bool> GenerateWaveform_Before()
        {
            return Task.Run(delegate
            {
                try
                {
                    Dispatcher.Invoke(delegate { SetUnloaded(); });
                    string filename = GetImagePath();
                    if (File.Exists(filename))
                    {
                        ImagePath = filename;
                        return false;
                    }
                    else
                    {
                        Dispatcher.Invoke(delegate {
                            if (File.Exists(ImagePath))
                                File.Delete(ImagePath);
                            ImagePath = filename;
                        });
                    }
                    

                    return true;
                }
                catch (Exception ex)
                {
                    MainWindow.MessageBoxError(ex, "Error on Before Generate Waveform");
                }
                return false;
            });
        }

        public async void GenerateWaveform_After()
        {
            try
            {
                if (await OpenImageAsync())
                    Dispatcher.Invoke(delegate { OnImageLoaded(); });
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on After Generate Waveform");
            }
        }


        public async Task<bool> GenerateWaveformAsync(bool force)
        {
            if (!IsEnabled)
                return false;
            return await Task.Run<bool>(delegate ()
            {
                try
                {

                    if (Thread != null && Thread.IsAlive)
                        Thread.Join();

                    //Thread = Thread.CurrentThread;
                    if (!GenerateWaveform(force))
                        return false;
                    return true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"{ex.Message}\r\n\r\n{ex.StackTrace}", "Error on GenerateWaveformsAsync",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }
            });
        }

        public bool GenerateWaveform(bool force = false)
        {
            if (WaveForm is null)
                return false;
            try
            {
                var points = WaveForm.GetAudioPoints();
                int width = (int)points.Last().X;
                var pen = new System.Drawing.Pen(System.Drawing.ColorTranslator.FromHtml(WaveForm.WAV_ZONE_COLOR));

                while (IsGenerating)
                {
                    // Wait for previous generating end;
                    Thread.Sleep(10);
                }
                Thread = new Thread(WaveForm.PointsToImage);
                Thread.IsBackground = true;
                Thread.Name = Recline.Filename;
                Thread.Start((object)(points, width, 100, ImagePath, pen));



                //WaveForm.PointsToImage((object)(points, width, 100, this));
                //IsGenerating = false;
                //if (!WaveForm.IsGenerated)
                //{
                //    MessageBox.Show($"{WaveForm.GeneratingException.Message}\r\n\r\n{WaveForm.GeneratingException.StackTrace}",
                //        "Error on image generation", MessageBoxButton.OK, MessageBoxImage.Error);
                //    return false;
                //}
                return true;
            }
            catch (Exception ex)
            {
                MainWindow.MessageBoxError(ex, "Error on Generate Waveform");
            }
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
                        Dispatcher.Invoke(() => { Undraw(); });

                        return false;
                    }
                    while (IsGenerating)
                    { }

                    Dispatcher.Invoke(() => {
                        if (OpenImage())
                            OnImageLoaded();
                    });
                    return true;
                }
                catch (Exception ex)
                {
                    MainWindow.MessageBoxError(ex, "Error on Open Image Async");
                    return false;
                }
            });
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
                Width = Length * ScaleX / 4;
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
