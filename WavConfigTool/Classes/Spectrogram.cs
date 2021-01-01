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
        /// sources:
        /// https://stackoverflow.com/questions/17404924/getting-pcm-values-of-wav-files
        /// https://stackoverflow.com/questions/53998592/c-sharp-application-sample-audio-from-audio-output-fft-algorithm-visualiz
        /// https://github.com/swharden/Csharp-Data-Visualization/tree/master/projects/18-01-11_microphone_spectrograph
        /// </summary>

        public static bool IsTested = false;

        public static int SpectrumShift = 0;
        public static double SpectrumScale = 1;
        public static int QualityX = 1;
        public static int QualityY = 1;

        public Bitmap MakeSectrogram(WaveForm waveForm, int width, int height)
        {
            if (!waveForm.IsEnabled)
                return null;

            var reader = new AudioFileReader(waveForm.Path);

            var bufferLength = 1024 * QualityX;
            int bytesRecorded;
            var spectrogramData = new List<double[]>();
            var bytesCount = reader.WaveFormat.BlockAlign;


            var waveBuffer = new float[bufferLength];
            while ((bytesRecorded = reader.Read(waveBuffer, 0, bytesCount)) > 0)
            {
                Complex[] tempbuffer = new Complex[waveBuffer.Length];

                for (int i = 0; i < tempbuffer.Length; i++)
                {
                    tempbuffer[i].X = (float)(waveBuffer[i] * FastFourierTransform.HammingWindow(i, waveBuffer.Length));
                }

                FastFourierTransform.FFT(true, (int)Math.Log(tempbuffer.Length, 2.0), tempbuffer);

                var fftOutput = new double[tempbuffer.Length / 2 - 1];
                for (int i = 0; i < fftOutput.Length; i++)
                {
                    var val = tempbuffer[i].X;
                    var pow = Math.Pow(val, 2);
                    var log = Math.Log(pow, 10);
                    fftOutput[i] = log;
                }
                spectrogramData.Add(fftOutput);
            }

            var spectrogramWidth = spectrogramData.Count;
            var spectrogramHeight = spectrogramData[0].Length;

            /// create a bitmap we will work with
            Bitmap bitmap = new Bitmap(spectrogramWidth, spectrogramHeight, PixelFormat.Format8bppIndexed);

            ///modify the indexed palette
            ColorPalette pallette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
                pallette.Entries[i] = System.Drawing.Color.FromArgb(255 - i, 255 - i/2, 40, 255 - i);
            bitmap.Palette = pallette;

            /// prepare to access data via the bitmapdata object
            BitmapData bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                                                    ImageLockMode.WriteOnly, bitmap.PixelFormat);

            /// create a byte array to reflect each pixel in the image
            byte[] pixels = new byte[bitmapData.Stride * bitmap.Height];

            /// fill pixel array with data
            for (int col = 0; col < spectrogramData.Count; col++)
            {
                for (int row = 0; row < spectrogramData[0].Length; row++)
                {
                    double pixelVal = Math.Pow(spectrogramData[col][row] * SpectrumScale, 2);
                    pixelVal -= SpectrumShift;
                    pixelVal = Math.Max(0, pixelVal);
                    pixelVal = Math.Min(255, pixelVal);

                    int bytePosition = (row) * bitmapData.Stride + col;
                    if (pixels.Length <= bytePosition)
                        break;
                    pixels[bytePosition] = (byte)(pixelVal);
                }
            }

            /// turn the byte array back into a bitmap
            Marshal.Copy(pixels, 0, bitmapData.Scan0, pixels.Length);
            bitmap.UnlockBits(bitmapData);

            reader.Close();
            reader.Dispose();

            spectrogramData.Clear();

            var widthToResize = width == 0 ? 1000 : width;
            var resizedBitmap = ImagesLibrary.ResizeImage(bitmap, widthToResize, height);
            bitmap.Dispose();

            //resizedBitmap.Save("spectrum.png", ImageFormat.Png);
            //return null;

            return resizedBitmap;
        }
    }
}
