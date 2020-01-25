using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Tools;
using YamlDotNet.Serialization;

namespace WavConfigCore.Reader
{

    public class ProjectReader
    {
        #region singleton base

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

        #endregion

        public Project Read(string filename)
        {
            var project = ReadYaml(filename);
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
                return ioProject == null ? null : GetProject(ioProject, filename);
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
            ioProject.WavOptions.WavPrefix = project.WavPrefix;
            ioProject.WavOptions.WavSuffix = project.WavSuffix;
            ioProject.WavOptions.UserScaleY = project.UserScaleY;
            ioProject.WavOptions.UserScaleX = project.UserScaleX;
            ioProject.OtoOptions.OtoPrefix = project.Prefix;
            ioProject.OtoOptions.OtoSuffix = project.Suffix;
            ioProject.OtoOptions.VowelDecay = project.VowelDecay;
            ioProject.OtoOptions.VowelAttack = project.VowelAttack;
            ioProject.OtoOptions.ConsonantAttack = project.ConsonantAttack;
            ioProject.ProjectOptions = new IOProjectOptions
            {
                LastPage = project.ProjectOptions.LastPage,
                PageSize = project.ProjectOptions.PageSize
            };
            if (project.Voicebank != null)
                ioProject.Voicebank = project.Voicebank.Location;
            if (project.Reclist != null)
                ioProject.Reclist = project.Reclist.Name;

            var wavConfigsList = new List<IOWavConfig>();
            foreach (var projectLine in project.ProjectLines)
            {
                var ioWavConfig = new IOWavConfig
                {
                    Rests = projectLine.RestPoints.ToArray(),
                    Vowels = projectLine.VowelPoints.ToArray(),
                    Consonants = projectLine.ConsonantPoints.ToArray()
                };
                if (projectLine.Recline != null)
                    ioWavConfig.File = projectLine.Recline.Name;
                wavConfigsList.Add(ioWavConfig);
            }
            ioProject.WavConfigs = wavConfigsList.ToArray();

            return ioProject;
        }

        private Project GetProject(IOProject ioProject, string projectDir)
        {
            var project = new Project
            {
                WavPrefix = ioProject.WavOptions.WavPrefix,
                WavSuffix = ioProject.WavOptions.WavSuffix,
                UserScaleY = ioProject.WavOptions.UserScaleY,
                UserScaleX = ioProject.WavOptions.UserScaleX,
                Prefix = ioProject.OtoOptions.OtoPrefix,
                Suffix = ioProject.OtoOptions.OtoSuffix,
                VowelDecay = ioProject.OtoOptions.VowelDecay,
                VowelAttack = ioProject.OtoOptions.VowelAttack,
                ConsonantAttack = ioProject.OtoOptions.ConsonantAttack,
                ProjectOptions = new ProjectOptions()
            };
            project.ProjectOptions.LastPage = ioProject.ProjectOptions.LastPage;
            project.ProjectOptions.PageSize = ioProject.ProjectOptions.PageSize;

            project.SetVoicebank(new Voicebank(Path.GetDirectoryName(projectDir), ioProject.Voicebank));
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
            }

            return project;
        }
    }


}
