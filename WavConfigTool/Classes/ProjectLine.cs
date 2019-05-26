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

        public Recline Recline { get; set; }
        public bool IsCompleted { get; set; }

        public int WavImageHash { get; set; }
        public WaveForm WaveForm { get; set; }

        public delegate void ProjectLineChangedEventHandler();
        public event ProjectLineChangedEventHandler ProjectLineChanged;
        public event ProjectLineChangedEventHandler ProjectLinePointsChanged;

        /// <summary>
        /// Возвращает true если файл существовал на момент чтения проекта или изменения голосового банка/реклиста
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        public ProjectLine(Recline recline)
        {
            ProjectLineChanged += delegate { CalculateZones(); };
            ProjectLinePointsChanged += delegate { CalculateZones(); };
            Recline = recline;
        }

        public void CallChanged()
        {
            ProjectLineChanged();
        }

        public static ProjectLine Read(Recline recline, string pds, string pvs, string pcs)
        {
            var projectLine = new ProjectLine(recline);
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
            var projectLine = new ProjectLine(recline)
            {
                RestPoints = new List<int>(),
                VowelPoints = new List<int>(),
                ConsonantPoints = new List<int>()
            };
            return projectLine;
        }

        public void ReclistAndVoicebankCheck(Reclist reclist, Voicebank voicebank)
        {
            IsEnabled = voicebank.IsSampleEnabled(Recline.Filename);
            if (IsEnabled)
                WavImageHash = $"{voicebank.Location}{Recline.Filename}{reclist.Name}{Settings.WAM}".GetHashCode();
            if (IsEnabled)
                WaveForm = new WaveForm(Path.Combine(voicebank.Location, Recline.Filename));
        }

        public void CalculateZones()
        {
            Sort();
            VowelZones = new List<Zone>();
            foreach (PhonemeType type in new[] { PhonemeType.Consonant, PhonemeType.Vowel})
            {
                var points = PointsOfType(type);
                var zones = ZonesOfType(type);
                zones.Clear();
                for (int i = 0; i + 1 < points.Count; i += 2)
                {
                    zones.Add(new Zone(points[i], points[i + 1]));
                }
            }
            RestZones = CalculateRestZones();
        }

        public List<Zone> CalculateRestZones()
        {
            // У первой и последней точки это не зона, а синглтоны, а в середине обычные зоны
            var zones = new List<Zone>();
            var points = PointsOfType(PhonemeType.Rest);
            if (zones.Count > 0)
                zones.Add(new Zone(points[0], points[0]));
            for (int i = 1; i + 2 < points.Count; i += 2)
                zones.Add(new Zone(points[i], points[i + 1]));
            if (RestPoints.Count > 1)
                zones.Add(new Zone(points.Last(), points.Last()));
            return zones;
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
        public bool ApplyZones(Phoneme phoneme)
        {
            int i = Recline.Phonemes.IndexOf(phoneme);
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

        public List<int> PointsOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantPoints :
                (type == PhonemeType.Rest ? RestPoints : VowelPoints);
        }

        public List<Zone> ZonesOfType(PhonemeType type)
        {
            return type == PhonemeType.Consonant ? ConsonantZones :
                (type == PhonemeType.Rest ? RestZones : VowelZones);
        }

        public int AddPoint(int position, PhonemeType type)
        {
            var points = PointsOfType(type);
            points.Add(position);
            points.Sort();
            ProjectLinePointsChanged();
            return points.IndexOf(position);
        }

        public (int,int) MovePoint(int position1, int position2, PhonemeType type)
        {
            var points = PointsOfType(type);
            int i = points.IndexOf(position1);
            if (i > -1)
            {
                points[i] = position2;
            }
            points.Sort();
            ProjectLinePointsChanged();
            return (i,points.IndexOf(position2));
        }

        public int DeletePoint(int position, PhonemeType type)
        {
            var points = PointsOfType(type);
            var i = points.IndexOf(position);
            points.Remove(position);
            ProjectLinePointsChanged();
            return i;
        }
    }
}
