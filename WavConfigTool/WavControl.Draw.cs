using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WavConfigTool
{
    public partial class WavControl : UserControl
    {

        public void Undraw()
        {
            WavImage.Source = null;
            GridCanvas.Children.Clear();
            ClearWavconfig();
        }



        void Draw(WavConfigPoint point, double x)
        {
            switch (point)
            {
                case WavConfigPoint.C:
                    if (Cs.Count < Recline.Consonants.Count * 2)
                        Cs.Add(x / ScaleX);
                    else return;
                    break;
                case WavConfigPoint.V:
                    if (Vs.Count < Recline.Vowels.Count * 2)
                        Vs.Add(x / ScaleX);
                    else return;
                    break;
                case WavConfigPoint.D:
                    if (Ds.Count < 2)
                        Ds.Add(x / ScaleX);
                    else return;
                    break;
            }
            ClearWavconfig();
            Draw();
            WavControlChanged();
        }

        void ClearWavconfig()
        {
            AreasCanvas.Children.Clear();
            MarkerCanvas.Children.Clear();

        }

        void DrawConfig()
        {
            Ds.Sort();
            Vs.Sort();
            Cs.Sort();

            ClearWavconfig();

            CheckCompleted();

            DrawD();
            DrawV();
            DrawC();

            DrawVZone();
            DrawCZone();
            DrawDZone();
        }

        void DrawV()
        {
            for (int i = 0; i < Vs.Count; i++)
            {
                WavMarker line;
                double pos = Vs[i];
                if (i / 2 < Recline.Vowels.Count)
                    line = new WavMarker((Vowel)Recline.Vowels[i / 2], pos, i % 2);
                else line = new WavMarker(new Vowel("{V}"), pos, i % 2);
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Vs.Remove(pos);
                    Draw();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Vs[Vs.IndexOf(pos)] = Math.Truncate(newpos);
                    Draw();
                    WavControlChanged();
                };
            }
        }
        void DrawC()
        {
            for (int i = 0; i < Cs.Count; i++)
            {
                double pos = Cs[i];
                WavMarker line;
                if (i / 2 < Recline.Consonants.Count)
                    line = new WavMarker((Consonant)Recline.Consonants[i / 2], pos, i % 2);
                else line = new WavMarker(new Consonant("{Cf}"), pos, i % 2);
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Cs.Remove(pos);
                    Draw();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Cs[Cs.IndexOf(pos)] = Math.Truncate(newpos);
                    Draw();
                    WavControlChanged();
                };
            }
        }
        void DrawD()
        {
            if (Ds != null && Ds.Count == 2)
            {
                if (Ds[0] < 0)
                    Ds[0] = 50;
                if (Ds[1] > Length)
                    Ds[1] = Length - 50;
            }
            for (int i = 0; i < Ds.Count; i++)
            {
                double pos = Ds[i];
                WavMarker line = new WavMarker(pos, i);
                Data[i] = line;
                MarkerCanvas.Children.Add(line);
                line.WavMarkerDelete += delegate
                {
                    MarkerCanvas.Children.Remove(line);
                    Ds.Remove(pos);
                    Draw();
                    WavControlChanged();
                };
                line.WavMarkerMoved += delegate (double newpos)
                {
                    Ds[Ds.IndexOf(pos)] = Math.Truncate(newpos);
                    Draw();
                    WavControlChanged();
                };
            }
        }

        void DrawVZone()
        {
            for (int i = 0; i + 1 < Vs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = VowelZoneBrush,
                    Points = new PointCollection
                    {
                        new Point((Vs[i]) * ScaleX, 50),
                        new Point((Vs[i] + Settings.FadeV) * ScaleX, 30),
                        new Point((Vs[i + 1] - Settings.FadeV) * ScaleX, 30),
                        new Point((Vs[i + 1]) * ScaleX, 50),
                        new Point((Vs[i + 1] - Settings.FadeV) * ScaleX, 70),
                        new Point((Vs[i] + Settings.FadeV) * ScaleX, 70)
                    },
                    Name = $"VZone{i / 2}{(i % 2 == 0 ? "In" : "Out")}",
                    Fill = FillVowelZoneBrush
                };
                AreasCanvas.Children.Add(Zone);
            }
        }
        void DrawDZone()
        {
            if (Ds.Count == 0) return;
            var x = Ds[0] * ScaleX;
            Polygon ZoneIn = new Polygon()
            {
                Fill = CutZoneBrush,
                Points = new PointCollection()
                {
                    new Point(0,0),
                    new Point(x,0),
                    new Point(x - Settings.FadeD * ScaleX,50),
                    new Point(x,100),
                    new Point(0,100)
                },
                Name = "DZoneIn",
                Opacity = 0.5
            };
            AreasCanvas.Children.Add(ZoneIn);
            if (Ds.Count == 1) return;
            x = Ds[1] * ScaleX;
            Polygon ZoneOut = new Polygon()
            {
                Fill = CutZoneBrush,
                Points = new PointCollection()
                {
                    new Point(Length * ScaleX,0),
                    new Point(x,0),
                    new Point(x + Settings.FadeD * ScaleX,50),
                    new Point(x,100),
                    new Point(Length * ScaleX,100)
                },
                Name = "DZoneOut",
                Opacity = 0.5
            };
            AreasCanvas.Children.Add(ZoneOut);
        }
        void DrawCZone()
        {

            for (int i = 0; i + 1 < Cs.Count; i += 2)
            {
                Polygon Zone = new Polygon()
                {
                    Stroke = CZoneBrush,
                    Points = new PointCollection
                    {
                        //new Point((Cs[i]) * ScaleX, 50),
                        //new Point((Cs[i] + Settings.FadeC) * ScaleX, 40),
                        //new Point((Cs[i + 1] - Settings.FadeC) * ScaleX, 40),
                        //new Point((Cs[i + 1]) * ScaleX, 50),
                        //new Point((Cs[i + 1] - Settings.FadeC) * ScaleX, 60),
                        //new Point((Cs[i] + Settings.FadeC) * ScaleX, 60)
                        new Point((Cs[i]) * ScaleX, 40),
                        new Point((Cs[i + 1]) * ScaleX, 40),
                        new Point((Cs[i + 1]) * ScaleX, 60),
                        new Point((Cs[i]) * ScaleX, 60),
                    },
                    Name = $"CZone{i / 2}{(i % 2 == 0 ? "In" : "Out")}",
                    Fill = FillCZoneBrush
                };
                AreasCanvas.Children.Add(Zone);
            }
        }

        void DrawOtoPreview()
        {
            Recline.Reclist.Aliases = new List<string>();
            string oto = GenerateOto();
            OtoPreviewWindow window = new OtoPreviewWindow(Recline.Filename);
            foreach (string line in oto.Split(new[] { '\r', '\n' }))
            {
                if (line.Length == 0) continue;
                var ops = line.Split('=');
                var ops2 = ops[1].Split(',');
                var ops3 = ops2.Skip(1);
                int[] opsi = ops3.Select(n => int.Parse(n)).ToArray();
                OtoPreviewControl control = new OtoPreviewControl(WavImage.Source, ops2[0], opsi, Length);
                window.Add(control);
            }
            window.ShowDialog();
        }


        void CheckCompleted()
        {
            if (!IsEnabled || !IsImageGenerated)
                return;
            IsCompleted = Ds.Count == 2 &&
                Vs.Count / 2 >= Recline.Vowels.Count &&
                Cs.Count / 2 >= Recline.Consonants.Count;
            if (IsCompleted)
                SetCompleted();
            else
                SetUncompleted();
        }

        public void SetCompleted()
        {
            WavCompleted.Visibility = Visibility.Visible;
            WavCompleted.Opacity = 1;
        }

        public void SetUncompleted()
        {
            WavCompleted.Opacity = 0;
        }

        public void SetUnloaded()
        {
            Undraw();
            SetUncompleted();
            //HorizontalAlignment = HorizontalAlignment.Stretch;
            //Opacity = 0.2;
            WavImage.Source = null;
            CanvasLoading.Visibility = Visibility.Visible;
            CanvasLoading.HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        public void SetLoaded()
        {
            HorizontalAlignment = HorizontalAlignment.Left;
            Visibility = Visibility.Visible;
            CanvasLoading.Visibility = Visibility.Hidden;
            //Opacity = 1;
        }
    }
}
