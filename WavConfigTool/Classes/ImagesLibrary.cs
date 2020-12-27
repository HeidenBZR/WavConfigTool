using System;
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
    public enum WavImageType
    {
        WAVEFORM,
        FRQ,
        SPECTRUM
    }

    public class ImagesLibrary
    {
        public delegate void ImagesLibrarySimpleHandler(WaveForm waveForm);
        public event ImagesLibrarySimpleHandler OnImageLoaded = delegate { };

        public ImagesLibrary()
        {
            images[WavImageType.WAVEFORM] = new Dictionary<string, Bitmap>();
            images[WavImageType.FRQ] = new Dictionary<string, Bitmap>();
            images[WavImageType.SPECTRUM] = new Dictionary<string, Bitmap>();
        }

        public void Clear()
        {
            images[WavImageType.WAVEFORM].Clear();
            images[WavImageType.FRQ].Clear();
            images[WavImageType.SPECTRUM].Clear();
        }

        public void RegisterWaveForm(WaveForm waveForm)
        {
            images[WavImageType.WAVEFORM][waveForm.Path] = null;
            images[WavImageType.FRQ][waveForm.Path] = null;
            images[WavImageType.SPECTRUM][waveForm.Path] = null;
        }

        public void Load(WaveForm waveForm, int height, string hash)
        {
            if (!waveForm.IsEnabled)
            {
                ClearWavformImages(waveForm);
                Console.WriteLine($"failed to load image with hash {hash}");
                OnImageLoaded(waveForm);
                return;
            }
            if (waveForm.ImageHash != hash)
            {
                ClearWavformImages(waveForm);
            }

            Console.WriteLine($"started load image with hash {hash}");
            var yScale = Settings.UserScaleY;
            LoadWaveForm(waveForm, height, yScale);
            LoadFrq(waveForm, height);

            Console.WriteLine($"finished load image with hash {hash}");
            OnImageLoaded(waveForm);
        }

        public void LoadSpectrum(WaveForm waveForm, int height, string hash)
        {
            if (!waveForm.IsEnabled)
            {
                ClearWavformImages(waveForm);
                Console.WriteLine($"failed to load spectrum with hash {hash}");
                OnImageLoaded(waveForm);
                return;
            }

            Console.WriteLine($"started load spectrum with hash {hash}");
            ClearWavformImageOfType(waveForm, WavImageType.SPECTRUM);
            LoadSpectrum(waveForm, waveForm.VisualWidth, height);

            Console.WriteLine($"finished load spectrum with hash {hash}");
            OnImageLoaded(waveForm);
        }

        public ImageSource TryGetImage(WaveForm waveForm, WavImageType type)
        {
            if (waveForm == null)
                return null;
            if (!images[type].ContainsKey(waveForm.Path))
                return null;
            if (images[type][waveForm.Path] == null)
                return null;
            return Bitmap2ImageSource(images[type][waveForm.Path]);
        }

        public void ClearWavformImages(WaveForm waveForm)
        {
            if (waveForm == null)
                return;
            ClearWavformImageOfType(waveForm, WavImageType.FRQ);
            ClearWavformImageOfType(waveForm, WavImageType.SPECTRUM);
            ClearWavformImageOfType(waveForm, WavImageType.WAVEFORM);
            RegisterWaveForm(waveForm);
        }

        public static ImageSource Bitmap2ImageSource(Bitmap bitmap)
        {
            var memory = new MemoryStream();
            bitmap.Save(memory, ImageFormat.Png);
            memory.Position = 0;

            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();
            bitmapImage.Freeze();

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

        private Dictionary<WavImageType, Dictionary<string, Bitmap>> images = new Dictionary<WavImageType, Dictionary<string, Bitmap>>();
        private readonly System.Drawing.Color waveformColor = System.Drawing.Color.FromArgb(255, 100, 200, 100);// "#64c864"
        private readonly Spectrogram spectrogram = new Spectrogram();

        private void LoadWaveForm(WaveForm waveForm, int height, double yScale)
        {
            if (HasImageOfType(waveForm, WavImageType.WAVEFORM))
                return;
            var bitmap = waveForm.DrawWaveform(height, waveformColor, yScale);
            if (bitmap == null)
                return;
            images[WavImageType.WAVEFORM][waveForm.Path] = bitmap;
        }

        private void LoadFrq(WaveForm waveForm, int height)
        {
            if (HasImageOfType(waveForm, WavImageType.FRQ))
                return;
            var frq = new Frq();
            frq.Load(waveForm.Path);
            if (frq.Points != null)
            {
                var visualPoints = frq.CalculateVisualPoints(waveForm, height);
                var bitmap = frq.DrawPoints(visualPoints, height);
                if (bitmap == null)
                    return;
                images[WavImageType.FRQ][waveForm.Path] = bitmap;
            }
        }

        private void LoadSpectrum(WaveForm waveForm, int width, int height)
        {
            if (HasImageOfType(waveForm, WavImageType.SPECTRUM))
                return;
            var bitmap = spectrogram.MakeSectrogram(waveForm, width, height);
            if (bitmap == null)
                return;
            images[WavImageType.SPECTRUM][waveForm.Path] = bitmap;
        }

        private void ClearWavformImageOfType(WaveForm waveForm, WavImageType type)
        {
            if (images[type].ContainsKey(waveForm.Path) && images[type][waveForm.Path] != null)
            {
                images[type][waveForm.Path].Dispose();
            }
        }

        private bool HasImageOfType(WaveForm waveForm, WavImageType type)
        {
            return images[type].ContainsKey(waveForm.Path) && images[type][waveForm.Path] != null;
        }
    }
}
