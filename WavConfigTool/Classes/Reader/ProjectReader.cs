using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Tools;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.Reader
{

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
            project.SetReplacer(ReplacerReader.Current.Read(ioProject.Replacer, project.Reclist));

            foreach (var ioWavConfig in ioProject.WavConfigs)
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


}
