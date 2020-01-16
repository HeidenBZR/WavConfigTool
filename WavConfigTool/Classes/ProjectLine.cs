using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WavConfigTool.Tools;

namespace WavConfigTool.Classes
{
    public class ProjectLine
    {
        public List<int> VowelPoints { get; set; } = new List<int>();
        public List<int> ConsonantPoints { get; set; } = new List<int>();
        public List<int> RestPoints { get; set; } = new List<int>();
        public List<Zone> VowelZones { get; private set; } = new List<Zone>();
        public List<Zone> ConsonantZones { get; private set; } = new List<Zone>();
        public List<Zone> RestZones { get; private set; } = new List<Zone>();

        public List<int> VirtualRestPoints => GetVirtualRestPoints();

        public Recline Recline { get; set; }
        public bool IsCompleted
        {
            get
            {
                return RestPoints != null && VowelPoints != null && ConsonantPoints != null &&
                    VirtualRestPoints.Count >= Recline.Rests.Count * 2 &&
                    ConsonantPoints.Count >= Recline.Consonants.Count * 2 &&
                    VowelPoints.Count >= Recline.Vowels.Count * 2;
            }
        }

        public string WavImageHash { get; set; }
        public WaveForm WaveForm { get; set; }

        public event SimpleHandler ProjectLineChanged;
        public event SimpleHandler ProjectLinePointsChanged;

        /// <summary>
        /// Возвращает true если файл существовал на момент чтения проекта или изменения голосового банка/реклиста
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        public ProjectLine()
        {
            ProjectLineChanged += delegate { CalculateZones(); };
            ProjectLinePointsChanged += delegate { CalculateZones(); SetHasZone(); };
        }

        public void SetPoints(IEnumerable<int> vowels, IEnumerable<int> consonants, IEnumerable<int> rests)
        {
            VowelPoints = vowels.ToList();
            ConsonantPoints = consonants.ToList();
            RestPoints = rests.ToList();
        }

        public void SetRecline(Recline recline)
        {
            Recline = recline;
            SetHasZone();
            ProjectLineChanged();
            ProjectLinePointsChanged();
        }

        public void CallChanged()
        {
            ProjectLineChanged();
        }

        public static ProjectLine Read(Recline recline, string pds, string pvs, string pcs)
        {
            var projectLine = new ProjectLine();
            projectLine.SetRecline(recline);
            if (pds.Length > 0)
                projectLine.RestPoints = pds.Split(' ').Select(n => int.Parse(n)).ToList();
            if (pvs.Length > 0)
                projectLine.VowelPoints = pvs.Split(' ').Select(n => int.Parse(n)).ToList();
            if (pcs.Length > 0)
                projectLine.ConsonantPoints = pcs.Split(' ').Select(n => int.Parse(n)).ToList();

            return projectLine;
        }

        public static ProjectLine CreateNewFromRecline(Recline recline)
        {
            var projectLine = new ProjectLine()
            {
                RestPoints = new List<int>(),
                VowelPoints = new List<int>(),
                ConsonantPoints = new List<int>(),
                Recline = recline
            };
            return projectLine;
        }

        public void ReclistAndVoicebankCheck(Reclist reclist, Voicebank voicebank)
        {
            if (!IsEnabled)
                return;
            WavImageHash = $"{voicebank.Name}_{reclist.Name}_{Settings.UserScaleX}x{Settings.UserScaleY}_{Project.Current.Prefix}_{Recline.Filename}_{Project.Current.Suffix}"; //.GetHashCode();
            WaveForm = new WaveForm(Path.Combine(voicebank.Fullpath, Recline.Filename));
        }

        public void CalculateZones()
        {
            Sort();
            VowelZones = new List<Zone>();
            foreach (PhonemeType type in new[] { PhonemeType.Consonant, PhonemeType.Vowel, PhonemeType.Rest })
            {
                var points = PointsOfType(type, false);
                var zones = ZonesOfType(type);
                zones.Clear();
                // HACK: Rests currently are working as Zone=Point when others are two points for zone.
                // So there are fake zones for start and end rest zones. Should be made better
                if (type == PhonemeType.Rest)
                    zones.Add(new Zone(0, 0));
                for (int i = 0; i + 1 < points.Count; i += 2)
                {
                    if (type == PhonemeType.Rest)
                    {
                        zones.Add(new Zone(points[i], points[i]));
                        zones.Add(new Zone(points[i + 1], points[i + 1]));
                    }
                    else
                    {
                        zones.Add(new Zone(points[i], points[i + 1]));
                    }
                }
                if (type == PhonemeType.Rest)
                    zones.Add(new Zone(0, 0));
            }
        }

        public override string ToString()
        {
            return $"{{{Recline.Filename} ({(VowelPoints.Count + ConsonantPoints.Count + RestPoints.Count)})}}";
        }

        /// <summary>
        /// Распределяет зоны в фонемы. Возвращает false если в строке проекта нет такой фонемы или недостаточо зон.
        /// </summary>
        /// <param name="phoneme"></param>
        /// <returns></returns>
        public bool ApplyZones(List<Phoneme> phonemes, Phoneme phoneme)
        {
            int i = phonemes.IndexOf(phoneme);
            if (i < 0)
                return false;
            var zones = ZonesOfType(phoneme.Type);
            if (zones.Count > i)
            {
                phoneme.Zone = zones[i];
                return true;
            }
            return false;
        }

        public void Sort()
        {
            RestPoints.Sort();
            VowelPoints.Sort();
            ConsonantPoints.Sort();
        }

        public List<int> PointsOfType(PhonemeType type, bool virtuals = true)
        {
            return type == PhonemeType.Consonant ? ConsonantPoints :
                (type == PhonemeType.Vowel ? VowelPoints : 
                (virtuals ? VirtualRestPoints : RestPoints)
            );
        }

        public List<Zone> ZonesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantZones :
                (type == PhonemeType.Rest ? RestZones : VowelZones);
        }

        public int AddPoint(int position, PhonemeType type)
        {
            var points = PointsOfType(type);
            var phonemes = Recline.PhonemesOfType(type);
            var neededCount = phonemes.Count * 2;
            if (points.Count >= neededCount)
                return -1;
            var realPhonemes = PointsOfType(type, virtuals: false);
            realPhonemes.Add(position);
            realPhonemes.Sort();
            ProjectLinePointsChanged();
            return realPhonemes.IndexOf(position);
        }

        public void SetHasZone()
        {
            foreach (var phonemeType in new[] { PhonemeType.Consonant, PhonemeType.Vowel, PhonemeType.Rest })
            {
                var points = PointsOfType(phonemeType);
                var phonemes = Recline.PhonemesOfType(phonemeType);
                for (int i = 0; i < phonemes.Count; i++)
                {
                    phonemes[i].HasZone = points.Count >= i * 2 + 2;
                }
            }
        }

        public (int, int) MovePoint(int position1, int position2, PhonemeType type)
        {
            var points = PointsOfType(type, virtuals: false);
            int i = points.IndexOf(position1);
            if (i > -1)
            {
                points[i] = position2;
            }
            points.Sort();
            SetHasZone();
            ProjectLinePointsChanged();
            return (i, points.IndexOf(position2));
        }

        public int DeletePoint(int position, PhonemeType type)
        {
            var points = PointsOfType(type, virtuals:false);
            var i = points.IndexOf(position);
            points.Remove(position);
            SetHasZone();
            ProjectLinePointsChanged();
            return i;
        }

        private List<int> GetVirtualRestPoints()
        {
            var points = new List<int>();
            if (RestPoints.Count > 0)
            {
                points.Add(0);
                points.Add(RestPoints[0]);
            }
            if (RestPoints.Count > 1)
            {
                points.AddRange(RestPoints.GetRange(1, RestPoints.Count - 1));
                if (WaveForm != null && WaveForm.VisualWidth != 0)
                    points.Add(Settings.ViewToRealX(WaveForm.VisualWidth));
            }
            return points;
        }
    }
}
