using System;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace WavConfigTool.Classes
{
    public class WavPlayer
    {
        public void Play(WaveForm waveForm, int startPosition, int length)
        {
            Stop();
            if (waveForm != null && waveForm.Path != null)
                Play(waveForm.Path, startPosition, length);
        }

        private WaveOutEvent SoundPlayer;

        private void Play(string filename, int startPosition, int length)
        {
            var file = new AudioFileReader(filename);
            var trimmed = new OffsetSampleProvider(file);
            trimmed.SkipOver = TimeSpan.FromMilliseconds(startPosition);
            trimmed.Take = TimeSpan.FromMilliseconds(length);

            SoundPlayer = new WaveOutEvent();
            SoundPlayer.Init(trimmed);
            SoundPlayer.Play();
        }

        private void Stop()
        {
            if (SoundPlayer != null)
            {
                SoundPlayer.Stop();
                SoundPlayer.Dispose();
            }
        }
    }
}
