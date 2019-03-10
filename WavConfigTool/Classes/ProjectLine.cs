using System.Collections.Generic;
using System.Linq;

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

        /// <summary>
        /// Возвращает true если файл существовал на момент чтения проекта или изменения голосового банка/реклиста
        /// </summary>
        public bool IsEnabled { get; set; } = false;

        public ProjectLine(Recline recline)
        {
            ProjectLineChanged += ProjectLine_OnProjectLineChanged;
            Recline = recline;
        }

        public void ProjectLine_OnProjectLineChanged()
        {
            CalculateZones();
        }
        
        public void CalculateZones()
        {
            VowelZones = new List<Zone>();
            for (int i = 0; i + 1 < VowelPoints.Count; i += 2)
                VowelZones.Add(new Zone(VowelPoints[i], VowelPoints[i + 1]));
            ConsonantZones = new List<Zone>();
            for (int i = 0; i + 1 < ConsonantPoints.Count; i += 2)
                ConsonantZones.Add(new Zone(ConsonantPoints[i], ConsonantPoints[i + 1]));
            // У первой и последней точки это не зона, а синглтоны, а в середине обычные зоны
            RestZones = new List<Zone>();
            if (RestPoints.Count > 0)
                RestZones.Add(new Zone(RestPoints[0], RestPoints[0]));
            for (int i = 1; i + 2 < RestPoints.Count; i += 2)
                RestZones.Add(new Zone(RestPoints[i], RestPoints[i + 1]));
            if (RestPoints.Count > 1)
                RestZones.Add(new Zone(RestPoints.Last(), RestPoints.Last()));
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
            if (phoneme.IsVowel)
            {
                if (VowelZones.Count > i)
                {
                    phoneme.Zone = VowelZones[i];
                    return true;
                }
            }
            if (phoneme.IsConsonant)
            {
                if (ConsonantZones.Count > i)
                {
                    phoneme.Zone = ConsonantZones[i];
                    return true;
                }
            }
            if (phoneme.IsRest)
            {
                if (RestZones.Count > i)
                {
                    phoneme.Zone = RestZones[i];
                    return true;
                }
            }
            return false;
        }

        public void Sort()
        {
            RestPoints.Sort();
            VowelPoints.Sort();
            ConsonantPoints.Sort();
        }

    }
}
