using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
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
            images[WavImageType.WAVEFORM] = new Dictionary<string, ImageSource>();
            images[WavImageType.FRQ] = new Dictionary<string, ImageSource>();
            images[WavImageType.SPECTRUM] = new Dictionary<string, ImageSource>();
        }

        public void Clear()
        {
            images[WavImageType.WAVEFORM].Clear();
            images[WavImageType.FRQ].Clear();
            images[WavImageType.SPECTRUM].Clear();
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
            LoadWaveForm(waveForm, height);
            LoadFrq(waveForm, height);
            LoadSpectrum(waveForm, height);
            Console.WriteLine($"finished load image with hash {hash}");
            OnImageLoaded(waveForm);
        }

        public ImageSource TryGetImage(WaveForm waveForm, WavImageType type)
        {
            if (waveForm == null)
                return null;
            if (!images[type].ContainsKey(waveForm.Path))
                return null;
            return images[type][waveForm.Path];
        }

        public void ClearWavformImages(WaveForm waveForm)
        {
            if (waveForm == null)
                return;
            if (images[WavImageType.WAVEFORM].ContainsKey(waveForm.Path))
                images[WavImageType.WAVEFORM].Remove(waveForm.Path);
            if (images[WavImageType.FRQ].ContainsKey(waveForm.Path))
                images[WavImageType.FRQ].Remove(waveForm.Path);
            if (images[WavImageType.SPECTRUM].ContainsKey(waveForm.Path))
                images[WavImageType.SPECTRUM].Remove(waveForm.Path);
        }

        private Dictionary<WavImageType, Dictionary<string, ImageSource>> images = new Dictionary<WavImageType, Dictionary<string, ImageSource>>();
        private readonly System.Drawing.Color waveformColor = System.Drawing.Color.FromArgb(255, 100, 200, 100);// "#64c864"

        private void LoadWaveForm(WaveForm waveForm, int height)
        {
            if (HasImageOfType(waveForm, WavImageType.WAVEFORM))
                return;
            var image = waveForm.DrawWaveform(height, waveformColor);
            if (image == null)
                return;
            images[WavImageType.WAVEFORM][waveForm.Path] = image;
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
                var image = frq.DrawPoints(visualPoints, height);
                if (image == null)
                    return;
                images[WavImageType.FRQ][waveForm.Path] = image;
            }
        }

        private void LoadSpectrum(WaveForm waveForm, int height)
        {

        }

        private bool HasImageOfType(WaveForm waveForm, WavImageType type)
        {
            return images[WavImageType.WAVEFORM].ContainsKey(waveForm.Path) && images[WavImageType.WAVEFORM][waveForm.Path] != null;
        }
    }
}
