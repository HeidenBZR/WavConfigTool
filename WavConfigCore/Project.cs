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
        public int PageSize = 5;
        public int LastPage = 0;
        public bool MustHideCompleted;
        public bool MustHideNotEnabled;
    }

    public class Project
    {
        #region variables

        public Dictionary<string, string> Options;
        public Dictionary<string, Oto> Otos { get; set; }
        public Oto[] OtoList { get => Otos.Values.ToArray(); }

        public Reclist Reclist { get => reclist; private set { reclist = value; ProjectChanged(); } }
        public Voicebank Voicebank { get => voicebank; private set { voicebank = value; ProjectChanged(); } }
        public Replacer Replacer { get => replacer; private set { replacer = value; ProjectChanged(); } }
        public ProjectOptions ProjectOptions { get; set; }

        public bool IsLoaded { get; set; } = false;
        public bool IsChangedAfterBackup { get; private set; }

        public int VowelDecay { get => vowelDecay; set { vowelDecay = value; ProjectChanged(); } }
        public int VowelAttack { get => vowelAttack; set { vowelAttack = value; ProjectChanged(); } }
        public int ConsonantAttack { get => consonantAttack; set { consonantAttack = value; ProjectChanged(); } }
        public int RestAttack { get => restAttack; set { restAttack = value; ProjectChanged(); } }
        public string Prefix { get => prefix; set { prefix = value; ProjectChanged(); } }
        public string Suffix { get => suffix; set { suffix = value; ProjectChanged(); } }
        public string WavPrefix { get => wavPrefix; set { wavPrefix = value; ProjectChanged(); } }
        public string WavSuffix { get => wavSuffix; set { wavSuffix = value; ProjectChanged(); } }
        public double UserScaleY { get => userScaleY; set { userScaleY = value; ProjectChanged(); } }
        public double UserScaleX { get => userScaleX; set { userScaleX = value; ProjectChanged(); } }

        public List<ProjectLine> ProjectLines { get => projectLines; set { projectLines = value; ProjectChanged(); } }
        public Dictionary<string, ProjectLine> ProjectLinesByFilename { get => projectLinesByFilename; set { projectLinesByFilename = value; ProjectChanged(); } }

        #endregion

        public event SimpleHandler ProjectChanged = delegate { };
        public event SimpleHandler ProjectLinesChanged = delegate { };
        public event SimpleHandler BeforeSave = delegate { };
        public event SimpleHandler AfterSave = delegate { };
        public event SimpleHandler SaveMe = delegate { };

        public Project(string filename = "")
        {
            ProjectOptions = new ProjectOptions();
            var projectDir = Directory.Exists(filename) ? Path.GetDirectoryName(filename) : "";
            voicebank = new Voicebank(projectDir, "");
            reclist = new Reclist();
            projectLines = new List<ProjectLine>();
            projectLinesByFilename = new Dictionary<string, ProjectLine>();
            Options = new Dictionary<string, string>();
            ProjectChanged += HandleProjectChanged;
            ProjectLinesChanged += HandleProjectLineChanged;
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
                ProjectChanged();
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
                Reader.ReclistReader.Current.Write(PathResolver.Current.Reclist(Reclist.Name + PathResolver.SETTINGS_EXT), Reclist);
            IsLoaded = Voicebank.IsLoaded && Reclist.IsLoaded;
            CheckEnabled();
            ProjectChanged();
        }

        public void SetReplacer(Replacer replacer)
        {
            Replacer = replacer;
            Reader.ReplacerReader.Current.Write(replacer, Reclist);
            ProjectChanged();
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
                Otos[newAlias] = oto;
            return (newAlias, oto);
        }

        public int AttackOfType(PhonemeType type)
        {
            if (type == PhonemeType.Consonant)
                return ConsonantAttack;
            else if (type == PhonemeType.Vowel)
                return VowelAttack;
            else
                return RestAttack;
        }

        public void ProcessLineAfterRead(ProjectLine projectLine)
        {
            projectLine.ProjectLineChanged += delegate { ProjectLinesChanged(); };
            projectLine.ProjectLinePointsChanged += delegate { FireSaveMe(); };
        }

        public void HandleBackupSaved()
        {
            IsChangedAfterBackup = false;
        }

        public void FireSaveMe()
        {
            if (!IsLoaded)
                return;
            IsChangedAfterBackup = true;
            BeforeSave();
            SaveMe();
            AfterSave();
        }

        #region private

        private Reclist reclist;
        private Voicebank voicebank;
        private Replacer replacer;
        private List<ProjectLine> projectLines;
        private Dictionary<string, ProjectLine> projectLinesByFilename;

        private int vowelDecay = 200;
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
            FireSaveMe();
        }

        private void HandleProjectLineChanged()
        {
            FireSaveMe();
        }

        private bool CheckForDuplicates(int i)
        {
            return i == 0 || Reclist.WavMask.MaxDuplicates == 0 || i < Reclist.WavMask.MaxDuplicates;
        }

        #endregion
    }
}
