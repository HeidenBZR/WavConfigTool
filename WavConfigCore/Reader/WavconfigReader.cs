using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Tools;

namespace WavConfigCore.Reader
{
    public class WavconfigReader
    {
        #region singleton base

        private static WavconfigReader current;
        private WavconfigReader() { }

        public static WavconfigReader Current
        {
            get
            {
                if (current == null)
                {
                    current = new WavconfigReader();
                }
                return current;
            }
        }

        #endregion

        public void Write(string path, Project project)
        {
            StringBuilder text = new StringBuilder();
            text.Append($"$Voicebank={project.Voicebank.Fullpath}\r\n");
            text.Append($"$Reclist={project.Reclist.Name}\r\n");
            text.Append($"$Suffix={project.Suffix}\r\n");
            text.Append($"$Prefix={project.Prefix}\r\n");
            text.Append($"$VowelDecay={project.VowelDecay}\r\n");
            text.Append($"$VowelAttack={project.VowelAttack}\r\n");
            text.Append($"$ConsonantAttack={project.ConsonantAttack}\r\n");
            text.Append($"$WavAmplitudeMultiplayer={project.UserScaleY.ToString("F2")}\r\n");
            foreach (var key in project.Options.Keys)
            {
                text.Append($"${key}={project.Options[key]}\r\n");
            }

            foreach (var projectLine in project.ProjectLines)
            {
                if (projectLine.RestPoints.Count == 0 && projectLine.VowelPoints.Count == 0 && projectLine.ConsonantPoints.Count == 0)
                    continue;
                text.Append($"{projectLine.Recline.Name}\r\n");
                text.Append($"{String.Join(" ", projectLine.RestPoints)}\r\n");
                text.Append($"{String.Join(" ", projectLine.VowelPoints) }\r\n");
                text.Append($"{String.Join(" ", projectLine.ConsonantPoints)}\r\n");
            }
            File.WriteAllText(path, text.ToString(), Encoding.UTF8);
        }

        void ReadOption(Project project, string line, string projectDir)
        {
            var pair = line.Split('=');
            var option = pair[0].Substring(1);
            var value = pair[1];
            switch (option)
            {
                case "Voicebank":
                    project.SetVoicebank(new Voicebank(Path.GetDirectoryName(projectDir), value));
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
                        project.UserScaleY = wavAmplitudeMultiplayer;
                    break;

                default:
                    project.Options[option] = value;
                    break;

            }
        }

        public Project Read(string location, string reclistName)
        {
            var project = new Project();
            string[] lines = File.ReadAllLines(location, Encoding.UTF8);
            project.Options = new Dictionary<string, string>();
            project.ProjectLines = new List<ProjectLine>();
            int i = 0;
            /// Совместимость со старыми сейвами без опций
            if (!lines[0].StartsWith("$"))
            {
                project.SetVoicebank(new Voicebank(Path.GetDirectoryName(location), lines[0]));
                i++;
            }
            /// Чтение опций
            for (; lines.Length > i && lines[i].StartsWith("$"); i++)
            {
                if (!lines[i].Contains("="))
                    continue;
                ReadOption(project, lines[i], location);

            }
            // TODO: select reclist to import
            project.SetReclist(ReclistReader.Current.Read(reclistName));
            project.ProjectLines = new List<ProjectLine>();
            var usedReclines = new List<Recline>();
            /// Чтение строк реклиста из проекта
            for (; i + 3 < lines.Length; i += 4)
            {
                var file = lines[i].Replace(".wav", "");
                var recline = project.Reclist.GetRecline(file);
                usedReclines.Add(recline);
                var projectLine = ProjectLine.Read(recline, lines[i + 1], lines[i + 2], lines[i + 3]);
                project.AddProjectLine(file, projectLine);
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
            return project;
        }
    }
}
