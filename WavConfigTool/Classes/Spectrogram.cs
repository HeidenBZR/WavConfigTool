using NAudio.Dsp;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace WavConfigTool.Classes
{

    /// buffer length needs to be a power of 2 for FFT to work nicely
    /// however, make the buffer too long and pitches aren't detected fast enough
    /// successful buffer sizes: 8192, 4096, 2048, 1024
    /// (some pitch detection algorithms need at least 2048)

    public class Spectrogram
    {
        /// <summary>
        /// Source: 
        /// https://github.com/swharden/Csharp-Data-Visualization/tree/master/projects/18-01-11_microphone_spectrograph
        /// </summary>

        public static bool IsTested = false;

        public void Start(WaveForm waveForm)
        {
            if (!waveForm.IsEnabled)
                return;
            //DrawRandomImage();

            // FFT/spectrogram configuration
            bufferLength = 1024 * 8; // must be a multiple of 2
            spectrogramHeight = bufferLength / 4; // why 4?

            // fill spectrogram data with empty values
            spectrogramData = new List<double[]>();

            // resize picturebox to accomodate data shape
            // var width = spectrogramData.Count;
            // var height = spectrogramData[0].Count;

            // start listening
            var reader = new WaveFileReader(waveForm.Path);
            var bitmap = MakeSpectrogram(reader);
            //bitmap = ImagesLibrary.ResizeImage(bitmap, 1300, 100);
            bitmap.Save($"spectrum_{Path.GetFileNameWithoutExtension(waveForm.Path)}.png", ImageFormat.Png);
        }

        private static List<double[]> spectrogramData; // columns are time points, rows are frequency points
        private static int spectrogramWidth;
        private static int spectrogramHeight;

        const double SCALE_FACTOR = 1;
        const bool LOG = false;

        private static Random rand = new Random();

        // spectrogram and FFT settings
        int bufferLength;


        /// <summary>
        /// sources:
        /// https://stackoverflow.com/questions/17404924/getting-pcm-values-of-wav-files
        /// https://stackoverflow.com/questions/53998592/c-sharp-application-sample-audio-from-audio-output-fft-algorithm-visualiz
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private Bitmap MakeSpectrogram(WaveFileReader reader)
        {
            var buffer = new byte[bufferLength];
            int bytesRecorded = 0;
            spectrogramData = new List<double[]>();
            var bytesCount = reader.WaveFormat.BlockAlign;
            var waveData = new float[reader.Length / bytesCount];

            var step = 0;
            while ((bytesRecorded = reader.Read(buffer, 0, bytesCount)) > 0)
            {
                // Converting the byte buffer in readable data
                float value;
                if (reader.WaveFormat.BitsPerSample == 16)
                    value = BitConverter.ToInt16(buffer, 0) / ((float)Int16.MaxValue + 1);
                else if (reader.WaveFormat.BitsPerSample == 32)
                    value = BitConverter.ToInt32(buffer, 0) / ((float)Int32.MaxValue + 1);
                else if (reader.WaveFormat.BitsPerSample == 64)
                    value = BitConverter.ToInt64(buffer, 0) / ((float)Int64.MaxValue + 1);
                else
                    return null;
                if (step % reader.WaveFormat.Channels != 0)
                    continue;
                waveData[step] = value;
                step++;
            }
            var waveStep = (int)Math.Pow(2, 4);
            spectrogramWidth = (int)(reader.Length / bytesCount / waveStep);
            for (var waveI = 0; waveI + bufferLength < waveData.Length; waveI += waveStep)
            {
                var waveBuffer = waveData.Skip(waveI).Take(bufferLength).ToArray();

                Complex[] tempbuffer = new Complex[waveBuffer.Length];

                for (int i = 0; i < tempbuffer.Length; i++)
                {
                    tempbuffer[i].X = (float)(waveBuffer[i] * FastFourierTransform.BlackmannHarrisWindow(i, waveBuffer.Length));
                    tempbuffer[i].Y = 0;
                }

                FastFourierTransform.FFT(true, (int)Math.Log(tempbuffer.Length, 2.0), tempbuffer);

                var fftOutput = new double[tempbuffer.Length / 2 - 1];
                for (int i = 0; i < fftOutput.Length; i++)
                {
                    var val = tempbuffer[i].X;
                    var pow = Math.Pow(val, 2);
                    var log = Math.Log10(pow);
                    var imag = tempbuffer[i].Y * tempbuffer[i].Y;
                    fftOutput[i] = 20 * log;
                }
                spectrogramData.Add(fftOutput);
            }

            return DrawFft();
        }

        private Bitmap DrawFft()
        {
            /// create a bitmap we will work with
            Bitmap bitmap = new Bitmap(spectrogramWidth, spectrogramHeight, PixelFormat.Format8bppIndexed);

            ///modify the indexed palette to make it grayscale
            ColorPalette pallette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
                pallette.Entries[i] = System.Drawing.Color.FromArgb(255, 255 - i, 0, 0);
            bitmap.Palette = pallette;

            /// prepare to access data via the bitmapdata object
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                    ImageLockMode.WriteOnly, bitmap.PixelFormat);

            /// create a byte array to reflect each pixel in the image
            byte[] pixels = new byte[bitmapData.Stride * bitmap.Height];

            /// I selected this manually to yield a number that "looked good"
            double scaleFactor = SCALE_FACTOR;
            /// fill pixel array with data
            for (int col = 0; col < spectrogramData.Count; col++)
            {
                for (int row = 0; row < spectrogramData[col].Length; row++)
                {
                    double pixelVal = Math.Abs(spectrogramData[col][row]) * scaleFactor;
                    pixelVal = Math.Max(0, pixelVal);
                    pixelVal = Math.Min(255, pixelVal);

                    int bytePosition = row * bitmapData.Stride + col;
                    if (pixels.Length <= bytePosition)
                        break;
                    pixels[bytePosition] = (byte)(pixelVal);
                }
            }

            /// turn the byte array back into a bitmap
            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            /// apply the bitmap to the picturebox
            return bitmap;
        }

        private void DrawRandomImage()
        {
            var width = 600;
            var height = 300;
            Bitmap bitmap = new Bitmap(width, height);

            var data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            var _buffer = new byte[data.Stride * bitmap.Height];
            var _random = new Random();

            for (int i = 0; i < 1000; i++)
            {
                var x = _random.Next(bitmap.Width);
                var y = _random.Next(bitmap.Height);

                var red = (byte)_random.Next(byte.MaxValue);
                var green = (byte)_random.Next(byte.MaxValue);
                var blue = (byte)_random.Next(byte.MaxValue);
                var alpha = (byte)_random.Next(byte.MaxValue);

                _buffer[y * data.Stride + x * 4] = blue;
                _buffer[y * data.Stride + x * 4 + 1] = green;
                _buffer[y * data.Stride + x * 4 + 2] = red;
                _buffer[y * data.Stride + x * 4 + 3] = alpha;
            }

            Marshal.Copy(_buffer, 0, data.Scan0, _buffer.Length);

            bitmap.UnlockBits(data);
            bitmap.Save("random.png", ImageFormat.Png);
        }

        private void Log(string message)
        {
            Console.WriteLine("FFT: " + message);
        }
    }
    class SampleAggregator
    {
        // FFT
        public event EventHandler<FftEventArgs> FftCalculated;
        public bool PerformFFT { get; set; }

        // This Complex is NAudio's own! 
        public Complex[] fftBuffer;
        private FftEventArgs fftArgs;
        private int fftPos;
        private int fftLength;
        private int m;

        public SampleAggregator(int fftLength)
        {
            if (!IsPowerOfTwo(fftLength))
            {
                throw new ArgumentException("FFT Length must be a power of two");
            }
            this.m = (int)Math.Log(fftLength, 2.0);
            this.fftLength = fftLength;
            this.fftBuffer = new Complex[fftLength];
            this.fftArgs = new FftEventArgs(fftBuffer);
        }

        bool IsPowerOfTwo(int x)
        {
            return (x & (x - 1)) == 0;
        }

        public void Add(float value)
        {
            if (PerformFFT && FftCalculated != null)
            {
                // Remember the window function! There are many others as well.
                fftBuffer[fftPos].X = (float)(value * FastFourierTransform.HammingWindow(fftPos, fftLength));
                fftBuffer[fftPos].Y = 0; // This is always zero with audio.
                fftPos++;
                if (fftPos >= fftLength)
                {
                    fftPos = 0;
                    FastFourierTransform.FFT(true, m, fftBuffer);
                    FftCalculated(this, fftArgs);
                }
            }
        }
    }

    public class FftEventArgs : EventArgs
    {
        public FftEventArgs(Complex[] result)
        {
            this.Result = result;
        }
        public Complex[] Result { get; private set; }
    }
}
