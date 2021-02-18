using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WavConfigTool.Classes
{
    public class ImagesLibrary
    {
        public delegate void ImagesLibrarySimpleHandler(WaveForm waveForm);
        public event ImagesLibrarySimpleHandler OnImageLoaded = delegate { };

        public void Clear()
        {
            images.Clear();
        }

        public void RegisterWaveForm(WaveForm waveForm)
        {
            var pack = new WavImagesPack();
            images.TryAdd(waveForm, pack);
        }

        public void Load(WaveForm waveForm, int height, string hash)
        {
            var hasImage = images.TryGetValue(waveForm, out var pack);
            if (!hasImage)
                throw new Exception();
            if (pack == null)
                throw new Exception();

            if (!waveForm.IsEnabled)
            {
                ClearWavformImages(waveForm);
                pack.IsLoading = false;
                pack.IsLoaded = false;
                Console.WriteLine($"ImagesLibrary: failed to load image with hash {hash}");
                OnImageLoaded(waveForm);
                return;
            }

            if (waveForm.ImageHash != hash)
                ClearWavformImages(waveForm);

            pack.IsLoading = true;
            pack.IsLoaded = false;

            Console.WriteLine($"ImagesLibrary: started load image with hash {hash}");
            LoadWaveForm(pack, waveForm, height, Settings.UserScaleY);
            LoadFrq(pack, waveForm, height);
            LoadSpectrum(pack, waveForm, waveForm.VisualWidth, height);
            Console.WriteLine($"ImagesLibrary: finished load image with hash {hash}");

            pack.IsLoading = false;
            pack.IsLoaded = true;

            OnImageLoaded(waveForm);
        }

        public void RequestLoadSpectrum(WaveForm waveForm, int height, string hash)
        {
            if (!waveForm.IsEnabled)
            {
                ClearWavformImages(waveForm);
                Console.WriteLine($"ImagesLibrary: failed to load spectrum with hash {hash}");
                OnImageLoaded(waveForm);
                return;
            }

            Console.WriteLine($"ImagesLibrary: started load spectrum with hash {hash}");
            images.TryGetValue(waveForm, out var pack);
            if (pack == null)
                throw new Exception();
            LoadSpectrum(pack, waveForm, waveForm.VisualWidth, height);

            Console.WriteLine($"ImagesLibrary: finished load spectrum with hash {hash}");
            OnImageLoaded(waveForm);
        }

        public WavImagesPack GetImagesPack(WaveForm waveForm)
        {
            if (waveForm == null)
                return null;
            images.TryGetValue(waveForm, out var pack);
            return pack;
        }

        public void ClearWavformImages(WaveForm waveForm)
        {
            if (waveForm == null)
                return;

            RegisterWaveForm(waveForm);
        }

        public static ImageSource Bitmap2ImageSource(Bitmap bitmap)
        {
            if (bitmap == null)
                return null;

            var bitmapImage = new BitmapImage();
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze();
            }

            return bitmapImage;
        }

        /// <summary>
        /// Resize the image to the specified width and height.
        /// source: https://stackoverflow.com/questions/1922040/how-to-resize-an-image-c-sharp
        /// </summary>
        /// <param name="image">The image to resize.</param>
        /// <param name="width">The width to resize to.</param>
        /// <param name="height">The height to resize to.</param>
        /// <returns>The resized image.</returns>
        public static Bitmap ResizeImage(Image image, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);
            var destImage = new Bitmap(width, height);

            destImage.SetResolution(image.HorizontalResolution, image.VerticalResolution);

            using (var graphics = Graphics.FromImage(destImage))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighSpeed;
                graphics.InterpolationMode = InterpolationMode.Low;
                graphics.SmoothingMode = SmoothingMode.HighSpeed;
                graphics.PixelOffsetMode = PixelOffsetMode.HighSpeed;

                using (var wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(image, destRect, 0, 0, image.Width, image.Height, GraphicsUnit.Pixel, wrapMode);
                }
            }

            return destImage;
        }

        private ConcurrentDictionary<WaveForm, WavImagesPack> images = new ConcurrentDictionary<WaveForm, WavImagesPack>();
        private readonly System.Drawing.Color waveformColor = System.Drawing.Color.FromArgb(255, 100, 200, 100);// "#64c864"
        private readonly Spectrogram spectrogram = new Spectrogram();

        private void LoadWaveForm(WavImagesPack pack, WaveForm waveForm, int height, double yScale)
        {
            var bitmap = waveForm.DrawWaveform(height, waveformColor, yScale, waveForm.Path);
            if (bitmap == null)
                return;
            pack.WavImage = bitmap;
        }

        private void LoadFrq(WavImagesPack pack, WaveForm waveForm, int height)
        {
            var frq = new Frq();
            frq.Load(waveForm.Path);
            if (frq.Points != null)
            {
                var visualPoints = frq.CalculateVisualPoints(waveForm, height);
                var bitmap = frq.DrawPoints(visualPoints, height, waveForm.Path);
                if (bitmap == null)
                    return;
                pack.Frq = bitmap;
            }
        }

        private void LoadSpectrum(WavImagesPack pack, WaveForm waveForm, int width, int height)
        {
            var bitmap = spectrogram.MakeSpectrogram(waveForm, width, height);
            if (bitmap == null)
                return;
            pack.Spectrogram = bitmap;
        }
    }

    public class WavImagesPack
    {
        public Bitmap WavImage;
        public Bitmap Spectrogram;
        public Bitmap Frq;
        public bool IsLoading;
        public bool IsLoaded;
    }
}
