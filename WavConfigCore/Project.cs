using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class ProjectOptions
    {
        public int OtoPageSize = 5;
        public int PageSize = 5;
        public int LastPage = 0;
        public bool MustHideCompleted;
        public bool MustHideNotEnabled;
    }

    public class ViewOptions
    {
        public bool DoShowPitch { get; set; } = true;
        public bool DoShowSpectrum { get; set; } = true;
        public bool DoShowWaveform { get; set; } = true;
    }

    public class Project
    {
        #region variables

        public Dictionary<string, string> Options;
        public Dictionary<string, Oto> Otos { get; set; }
        public Oto[] OtoList { get => Otos.Values.ToArray(); }

        public Reclist Reclist { get => reclist; private set { reclist = value; OnProjectChanged(); } }
        public Voicebank Voicebank { get => voicebank; private set { voicebank = value; OnProjectChanged(); } }
        public Replacer Replacer { get => replacer; private set { replacer = value; OnProjectChanged(); } }
        public ProjectOptions ProjectOptions { get; set; }
        public ViewOptions ViewOptions { get; set; }

        public bool IsLoaded { get; set; } = false;
        public bool IsChangedAfterBackup { get; private set; }

        public int DecayR { get => restDecay; set { restDecay = value; OnProjectChanged(); } }
        public int DecayV { get => vowelDecay; set { vowelDecay = value; OnProjectChanged(); } }
        public int DecayC { get => consonantDecay; set { consonantDecay = value; OnProjectChanged(); } }
        public int AttackR { get => restAttack; set { restAttack = value; OnProjectChanged(); } }
        public int AttackV { get => vowelAttack; set { vowelAttack = value; OnProjectChanged(); } }
        public int AttackC { get => consonantAttack; set { consonantAttack = value; OnProjectChanged(); } }
        public string Prefix { get => prefix; set { prefix = value; OnProjectChanged(); } }
        public string Suffix { get => suffix; set { suffix = value; OnProjectChanged(); } }
        public string WavPrefix { get => wavPrefix; set { wavPrefix = value; OnProjectChanged(); } }
        public string WavSuffix { get => wavSuffix; set { wavSuffix = value; OnProjectChanged(); } }
        public double UserScaleY { get => userScaleY; set { userScaleY = value; OnProjectChanged(); } }
        public double UserScaleX { get => userScaleX; set { userScaleX = value; OnProjectChanged(); } }

        public List<ProjectLine> ProjectLines { get => projectLines; set { projectLines = value; OnProjectChanged(); } }
        public Dictionary<string, ProjectLine> ProjectLinesByFilename { get => projectLinesByFilename; set { projectLinesByFilename = value; OnProjectChanged(); } }

        #endregion

        public event SimpleHandler OnProjectChanged = delegate { };
        public event SimpleHandler OnProjectLinesChanged = delegate { };
        public event SimpleHandler OnBeforeSave = delegate { };

        public Project(string filename = "")
        {
            ProjectOptions = new ProjectOptions();
            ViewOptions = new ViewOptions();
            voicebank = new Voicebank(PathResolver.Current.TryGetDirectoryName(filename), "");
            reclist = new Reclist();

            projectLines = new List<ProjectLine>();
            projectLinesByFilename = new Dictionary<string, ProjectLine>();
            Options = new Dictionary<string, string>();
            OnProjectChanged += HandleProjectChanged;
            OnProjectLinesChanged += HandleProjectLineChanged;
            IsLoaded = false;
            ResetOto();
        }

        public void SetVoicebank(Voicebank voicebank)
        {
            try
            {
                Voicebank = voicebank;
                IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
                CheckEnabled();
                OnProjectChanged();
            }
            catch (Exception ex)
            {
                Debug.Log(ex, "Error on SetVoicebank");
            }
        }

        public void SetReclist(Reclist reclist)
        {
            Reclist = reclist;
            if (Reclist.IsLoaded)
                Reader.ReclistReader.Current.WriteWithName(Reclist);
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
            CheckEnabled();
            OnProjectChanged();
        }

        public void SetReplacer(Replacer replacer)
        {
            Replacer = replacer;
            if (reclist.IsLoaded)
                Reader.ReplacerReader.Current.Write(replacer, Reclist);
            OnProjectChanged();
        }

        public void CheckEnabled()
        {
            if (!IsLoaded)
                return;

            foreach (var projectLine in ProjectLines)
            {
                projectLine.IsEnabled = false;
            }
            foreach (var recline in Reclist.Reclines)
            {
                if (!ProjectLinesByFilename.ContainsKey(recline.Name))
                {
                    AddProjectLine(recline.Name, new ProjectLine());
                }
                if (ProjectLinesByFilename.TryGetValue(recline.Name, out var projectLine))
                {
                    projectLine.Recline = recline;
                    projectLine.OnUpdateEnabledRequested += () =>
                    {
                        projectLine.IsEnabled = Voicebank.IsSampleEnabled(projectLine.Recline.Name, wavPrefix, wavSuffix);
                    };
                    projectLine.OnUpdateZonesRequested += () =>
                    {
                        Reclist.ApplyZones(projectLine);
                    };
                    projectLine.UpdateEnabled();
                    projectLine.UpdateZones();
                    ProcessLineAfterRead(projectLine);
                }
            }
        }

        public void AddProjectLine(string filename, ProjectLine projectLine)
        {
            ProjectLines.Add(projectLine);
            ProjectLinesByFilename[filename] = projectLine;
            ProcessLineAfterRead(projectLine);
        }

        public void ResetOto()
        {
            Otos = new Dictionary<string, Oto>();
        }

        public (string, Oto) AddOto(Oto oto)
        {
            var newAlias = oto.Alias;
            int i = 0;
            if (Otos.ContainsKey(oto.Alias))
            {
                do
                {
                    i++;
                    newAlias = $"{oto.Alias} ({i})";
                    oto.Number = i;
                }
                while (Otos.ContainsKey(newAlias));
            }
            if (CheckForDuplicates(i))
            {
                Otos[newAlias] = oto;
                return (newAlias, oto);
            }
            return (null, null);
        }

        public int AttackOfType(PhonemeType type)
        {
            if (type == PhonemeType.Consonant)
                return AttackC;
            else if (type == PhonemeType.Vowel)
                return AttackV;
            else
                return AttackR;
        }

        public int DecayOfType(PhonemeType type)
        {
            if (type == PhonemeType.Consonant)
                return DecayC;
            else if (type == PhonemeType.Vowel)
                return DecayV;
            else
                return DecayR;
        }

        public void ProcessLineAfterRead(ProjectLine projectLine)
        {
            projectLine.ProjectLineChanged += delegate { OnProjectLinesChanged(); };
            projectLine.ProjectLinePointsChanged += delegate { FireChanged(); };
        }

        public void HandleBackupSaved()
        {
            IsChangedAfterBackup = false;
            OnProjectChanged();
        }

        public void FireChanged()
        {
            if (!IsLoaded)
                return;
            OnProjectChanged();
        }

        public void FireBeforeSave()
        {
            OnBeforeSave();
        }

        #region private

        private Reclist reclist;
        private Voicebank voicebank;
        private Replacer replacer;
        private List<ProjectLine> projectLines;
        private Dictionary<string, ProjectLine> projectLinesByFilename;

        private int vowelDecay = 170;
        private int consonantDecay = 80;
        private int restDecay = 250;
        private int vowelAttack = 60;
        private int consonantAttack = 30;
        private int restAttack = 40;
        private string prefix = "";
        private string suffix = "";
        private double userScaleY = 1;
        private double userScaleX = 1;
        private string wavPrefix = "";
        private string wavSuffix = "";

        private void HandleProjectChanged()
        {
            IsChangedAfterBackup = true;
        }

        private void HandleProjectLineChanged()
        {
            FireChanged();
        }

        private bool CheckForDuplicates(int i)
        {
            return i == 0 || Reclist.WavMask.MaxDuplicates == -1 || i < Reclist.WavMask.MaxDuplicates;
        }

        #endregion
    }
}
