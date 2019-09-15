using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.IO
{
    class WConfigReader
    {
        private static WConfigReader current;
        private WConfigReader() { }

        public static WConfigReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new WConfigReader();
                }
                return current;
            }
        }

        public void Write(string path, Project project)
        {
            StringBuilder text = new StringBuilder();
            text.Append($"$Voicebank={project.Voicebank.Location}\r\n");
            text.Append($"$Reclist={project.Reclist.Name}\r\n");
            text.Append($"$Suffix={project.Suffix}\r\n");
            text.Append($"$Prefix={project.Prefix}\r\n");
            text.Append($"$VowelDecay={project.VowelDecay}\r\n");
            text.Append($"$VowelAttack={project.VowelAttack}\r\n");
            text.Append($"$ConsonantAttack={project.ConsonantAttack}\r\n");
            text.Append($"$WavAmplitudeMultiplayer={project.WavAmplitudeMultiplayer.ToString("F2")}\r\n");
            foreach (var key in project.Options.Keys)
            {
                text.Append($"${key}={project.Options[key]}\r\n");
            }

            foreach (var projectLine in project.ProjectLines)
            {
                if (projectLine.RestPoints.Count == 0 && projectLine.VowelPoints.Count == 0 && projectLine.ConsonantPoints.Count == 0)
                    continue;
                text.Append($"{projectLine.Recline.Filename}\r\n");
                text.Append($"{String.Join(" ", projectLine.RestPoints)}\r\n");
                text.Append($"{String.Join(" ", projectLine.VowelPoints) }\r\n");
                text.Append($"{String.Join(" ", projectLine.ConsonantPoints)}\r\n");
            }
            File.WriteAllText(path, text.ToString(), Encoding.UTF8);
        }

        void ReadOption(Project project, string line)
        {
            var pair = line.Split('=');
            var option = pair[0].Substring(1);
            var value = pair[1];
            switch (option)
            {
                case "Voicebank":
                    project.SetVoicebank(new Voicebank(value));
                    break;

                case "Reclist":
                    project.SetReclist(ReclistReader.Current.Read(value));
                    break;

                case "Suffix":
                    project.Suffix = value;
                    break;

                case "Prefix":
                    project.Prefix = value;
                    break;

                case "VowelDecay":
                    if (int.TryParse(value, out int vowelDecay))
                        project.VowelDecay = vowelDecay;
                    break;

                case "VowelAttack":
                    if (int.TryParse(value, out int vowelAttack))
                        project.VowelAttack = vowelAttack;
                    break;

                case "ConsonantAttack":
                    if (int.TryParse(value, out int consonantAttack))
                        project.ConsonantAttack = consonantAttack;
                    break;

                case "WavAmplitudeMultiplayer":
                    if (double.TryParse(value, out double wavAmplitudeMultiplayer))
                        project.WavAmplitudeMultiplayer = wavAmplitudeMultiplayer;
                    break;

                default:
                    project.Options[option] = value;
                    break;

            }
        }

        public Project Read(string location)
        {
            var project = new Project();
            string[] lines = File.ReadAllLines(location, Encoding.UTF8);
            project.Options = new Dictionary<string, string>();
            project.ProjectLines = new List<ProjectLine>();
            var _projectLinesByFilename = new Dictionary<string, ProjectLine>();
            int i = 0;
            /// Совместимость со старыми сейвами без опций
            if (!lines[0].StartsWith("$"))
            {
                project.SetVoicebank(new Voicebank(lines[0]));
                i++;
            }
            /// Чтение опций
            for (; lines.Length > i && lines[i].StartsWith("$"); i++)
            {
                if (!lines[i].Contains("="))
                    continue;
                ReadOption(project, lines[i]);

            }
            project.ProjectLines = new List<ProjectLine>();
            var usedReclines = new List<Recline>();
            /// Чтение строк реклиста из проекта
            for (; i + 3 < lines.Length; i += 4)
            {
                var recline = project.Reclist.GetRecline(lines[i]);
                usedReclines.Add(recline);
                var projectLine = ProjectLine.Read(recline, lines[i + 1], lines[i + 2], lines[i + 3]);
                project.ProcessLineAfterRead(projectLine);
            }
            /// Чтение строк реклиста, которых нет в проекте 
            if (project.Reclist != null)
            {
                foreach (var recline in project.Reclist.Reclines)
                {
                    if (!usedReclines.Contains(recline))
                    {
                        project.ProcessLineAfterRead(ProjectLine.CreateNewFromRecline(recline));
                    }
                }
            }
            // TODO: сортировать по реклисту
            return project;
        }

        public async void SaveAsync(Project project, string path)
        {
            await Task.Run(() => Write(path, project));
        }

        public Project OpenBackup()
        {
            if (!Settings.IsUnsaved || !File.Exists(Settings.TempProject))
                return null;
            var project = Open(Settings.TempProject);
            Settings.IsUnsaved = true;
            return project;
        }

        public Project OpenLast()
        {
            if (!File.Exists(Settings.ProjectFile))
                return null;
            var project = Open(Settings.ProjectFile);
            return project;
        }

        public Project Open(string project_path)
        {

            if (!File.Exists(project_path))
                return null;
            var project = Read(project_path);
            project.IsLoaded = project.Reclist.IsLoaded && project.Voicebank.IsLoaded;
            project.SetOtoGenerator(new OtoGenerator(project.Reclist, project));
            Settings.ProjectFile = project_path;
            return project;
        }
    }

    class ProjectReader
    {
        private static ProjectReader current;
        private ProjectReader() { }

        public static ProjectReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new ProjectReader();
                }
                return current;
            }
        }

        public Project Read(string filename)
        {
            var project = ReadYaml(filename);
            Settings.ProjectFile = project != null ? filename : Settings.ProjectFile;
            return project ?? new Project();
        }

        private Project ReadYaml(string filename)
        {
            using (var fileStream = new FileStream(filename, FileMode.OpenOrCreate))
            {
                var serializer = new Deserializer();
                IOProject ioProject = null;
                try
                {
                    ioProject = serializer.Deserialize(new StreamReader(fileStream, Encoding.UTF8), typeof(IOProject)) as IOProject;
                }
                catch { }
                return ioProject == null ? null : GetProject(ioProject);
            }
        }

        public void Write(string filename, Project project)
        {
            WriteYaml(filename, project);
        }

        private void WriteYaml(string filename, Project project)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
                var ioProject = GetIOProject(project);
                var serializer = new Serializer();
                serializer.Serialize(writer, ioProject, typeof(IOProject));
            }
        }

        private IOProject GetIOProject(Project project)
        {
            var ioProject = new IOProject();
            if (project.Voicebank != null)
                ioProject.Voicebank = project.Voicebank.Location;
            if (project.Reclist != null)
                ioProject.Reclist = project.Reclist.Name;
            ioProject.WavOptions.WavPrefix = project.WavPrefix;
            ioProject.WavOptions.WavSuffix = project.WavSuffix;
            ioProject.WavOptions.WavAmplitudeMultiplayer = project.WavAmplitudeMultiplayer;
            ioProject.OtoOptions.OtoPrefix = project.Prefix;
            ioProject.OtoOptions.OtoSuffix = project.Suffix;
            ioProject.OtoOptions.VowelDecay = project.VowelDecay;
            ioProject.OtoOptions.VowelAttack = project.VowelAttack;
            ioProject.OtoOptions.ConsonantAttack = project.ConsonantAttack;
            ioProject.ProjectOptions = new IOProjectOptions();
            ioProject.ProjectOptions.LastPage = project.ProjectOptions.LastPage;
            ioProject.ProjectOptions.PageSize = project.ProjectOptions.PageSize;

            var wavConfigsList = new List<IOWavConfig>();
            foreach (var projectLine in project.ProjectLines)
            {
                var ioWavConfig = new IOWavConfig();
                ioWavConfig.Rests = projectLine.RestPoints.ToArray();
                ioWavConfig.Vowels = projectLine.VowelPoints.ToArray();
                ioWavConfig.Consonants = projectLine.ConsonantPoints.ToArray();
                if (projectLine.Recline != null)
                    ioWavConfig.File = projectLine.Recline.Filename;
                wavConfigsList.Add(ioWavConfig);
            }
            ioProject.WavConfigs = wavConfigsList.ToArray();

            return ioProject;
        }

        private Project GetProject(IOProject ioProject)
        {
            var project = new Project();

            project.WavPrefix = ioProject.WavOptions.WavPrefix;
            project.WavSuffix = ioProject.WavOptions.WavSuffix;
            project.WavAmplitudeMultiplayer = ioProject.WavOptions.WavAmplitudeMultiplayer;
            project.Prefix = ioProject.OtoOptions.OtoPrefix;
            project.Suffix = ioProject.OtoOptions.OtoSuffix;
            project.VowelDecay = ioProject.OtoOptions.VowelDecay;
            project.VowelAttack = ioProject.OtoOptions.VowelAttack;
            project.ConsonantAttack = ioProject.OtoOptions.ConsonantAttack;
            project.ProjectOptions = new ProjectOptions();
            project.ProjectOptions.LastPage = ioProject.ProjectOptions.LastPage;
            project.ProjectOptions.PageSize = ioProject.ProjectOptions.PageSize;

            project.SetVoicebank(new Voicebank(ioProject.Voicebank));
            project.SetReclist(ReclistReader.Current.Read(ioProject.Reclist));

            foreach (var ioWavConfig in  ioProject.WavConfigs)
            {
                if (!project.ProjectLinesByFilename.ContainsKey(ioWavConfig.File))
                {
                    project.AddProjectLine(ioWavConfig.File, new ProjectLine());
                }
                var projectLine = project.ProjectLinesByFilename[ioWavConfig.File];
                projectLine.SetPoints(ioWavConfig.Vowels, ioWavConfig.Consonants, ioWavConfig.Rests);
                projectLine.ReclistAndVoicebankCheck(project.Reclist, project.Voicebank);
            }

            return project;
        }
    }


    [Serializable]
    public class IOWavOptions
    {
        public string WavPrefix;
        public string WavSuffix;
        public double WavAmplitudeMultiplayer = 1;
    }
    [Serializable]
    public class IOOtoOptions
    {
        public string OtoPrefix;
        public string OtoSuffix;
        public int VowelDecay = 80;
        public int VowelAttack = 60;
        public int ConsonantAttack = 40;
    }

    [Serializable]
    public class IOWavConfig
    {
        public string File = "";
        public int[] Vowels = new int[0];
        public int[] Consonants = new int[0];
        public int[] Rests = new int[0];
    }

    [Serializable]
    public class IOProjectOptions
    {
        public int PageSize = 5;
        public int LastPage = 0;
    }

    [Serializable]
    public class IOProject
    {
        public string Voicebank;
        public string Reclist;
        public IOWavOptions WavOptions = new IOWavOptions();
        public IOOtoOptions OtoOptions = new IOOtoOptions();
        public IOProjectOptions ProjectOptions = new IOProjectOptions();
        public IOWavConfig[] WavConfigs = new IOWavConfig[0];
    }

}
