using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WavConfigCore.Tools;
using YamlDotNet.Serialization.NodeDeserializers;

namespace WavConfigCore
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
        public int Width { get; set; }

        public Recline Recline { get; set; }

        public event SimpleHandler ProjectLineChanged = delegate { };
        public event SimpleHandler ProjectLinePointsChanged = delegate { };
        public event SimpleHandler OnUpdateEnabledRequested = delegate { };
        public event SimpleHandler OnUpdateZonesRequested = delegate { };

        /// <summary>
        /// Возвращает true если файл существовал на момент чтения проекта или изменения голосового банка/реклиста
        /// </summary>
        public bool IsEnabled { get; set; } = false;
        public bool IsCompleted { get; set; } = false;

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

        public void UpdateEnabled()
        {
            OnUpdateEnabledRequested();
        }

        public void UpdateZones()
        {
            OnUpdateZonesRequested();
        }

        public bool IsEmpty()
        {
            return RestPoints.Count == 0 && ConsonantPoints.Count == 0 && VowelPoints.Count == 0;
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

        public ProjectLine()
        {

        }

        public void SetPoints(IEnumerable<int> vowels, IEnumerable<int> consonants, IEnumerable<int> rests)
        {
            VowelPoints = vowels.ToList();
            ConsonantPoints = consonants.ToList();
            RestPoints = rests.ToList();
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
            UpdateZones();
            ProjectLinePointsChanged();
            return realPhonemes.IndexOf(position);
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
            UpdateZones();
            ProjectLinePointsChanged();
            return (i, points.IndexOf(position2));
        }

        public int DeletePoint(int position, PhonemeType type)
        {
            var points = PointsOfType(type, virtuals:false);
            var i = points.IndexOf(position);
            points.Remove(position);
            UpdateZones();
            ProjectLinePointsChanged();
            return i;
        }

        public override string ToString()
        {
            return $"{{{Recline.Name} ({(VowelPoints.Count + ConsonantPoints.Count + RestPoints.Count)})}}";
        }

        #region private

        // TODO: move to WavConfigTool
        private List<int> GetVirtualRestPoints()
        {
            var points = new List<int>();
            var firstSkipped = Recline.Phonemes.Count > 0 && Recline.Phonemes[0].IsSkipped;
            if (RestPoints.Count > 0)
            {
                if (!firstSkipped)
                {
                    points.Add(0);
                }
                points.Add(RestPoints[0]);
            }
            if (RestPoints.Count > 1 || firstSkipped && RestPoints.Count > 0)
            {
                points.AddRange(RestPoints.GetRange(1, RestPoints.Count - 1));
                if (Width != 0)
                    points.Add(Width);
            }
            return points;
        }

        private void SetRecline(Recline recline)
        {
            Recline = recline;
            OnUpdateZonesRequested();
            ProjectLineChanged();
            ProjectLinePointsChanged();
        }

        #endregion
    }
}
