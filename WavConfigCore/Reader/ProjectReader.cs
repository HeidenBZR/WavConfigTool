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
            var ioProject = ReadYaml(filename);
            return ioProject != null ? GetProject(ioProject, filename) : new Project();
        }

        public IOProject ReadYaml(string filename)
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
                return ioProject;
            }
        }

        public void Write(string filename, Project project)
        {
            WriteYaml(filename, GetIOProject(project));
        }

        public void WriteYaml(string filename, IOProject ioProject)
        {
            using (var writer = new StreamWriter(filename, false, Encoding.UTF8))
            {
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
            ioProject.OtoOptions.VowelDecay = project.DecayV;
            ioProject.OtoOptions.ConsonantDecay = project.DecayC;
            ioProject.OtoOptions.RestDecay = project.DecayR;
            ioProject.OtoOptions.VowelAttack = project.AttackV;
            ioProject.OtoOptions.ConsonantAttack = project.AttackC;
            ioProject.OtoOptions.RestAttack = project.AttackR;
            ioProject.ProjectOptions = new IOProjectOptions
            {
                LastPage = project.ProjectOptions.LastPage,
                PageSize = project.ProjectOptions.PageSize,
                OtoPageSize = project.ProjectOptions.OtoPageSize,
                MustHideNotEnabled = project.ProjectOptions.MustHideNotEnabled,
                MustHideCompleted = project.ProjectOptions.MustHideCompleted
            };
            ioProject.ViewOptions = new IOViewOptions
            {
                DoShowPitch = project.ViewOptions.DoShowPitch,
                DoShowSpectrum = project.ViewOptions.DoShowSpectrum,
                DoShowWaveform = project.ViewOptions.DoShowWaveform
            };
            if (project.Voicebank != null)
                ioProject.Voicebank = project.Voicebank.Location;
            if (project.Reclist != null)
                ioProject.Reclist = project.Reclist.Name;

            var wavConfigsList = new List<IOWavConfig>();
            foreach (var projectLine in project.ProjectLines)
            {
                if (projectLine.IsEmpty())
                    continue;
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
                DecayV = ioProject.OtoOptions.VowelDecay,
                DecayC = ioProject.OtoOptions.ConsonantDecay,
                DecayR = ioProject.OtoOptions.RestDecay,
                AttackV = ioProject.OtoOptions.VowelAttack,
                AttackC = ioProject.OtoOptions.ConsonantAttack,
                AttackR = ioProject.OtoOptions.RestAttack,
                ProjectOptions = new ProjectOptions(),
                ViewOptions = new ViewOptions()
            };
            project.ProjectOptions.LastPage = ioProject.ProjectOptions.LastPage;
            project.ProjectOptions.PageSize = ioProject.ProjectOptions.PageSize;
            project.ProjectOptions.OtoPageSize = ioProject.ProjectOptions.OtoPageSize;
            project.ProjectOptions.MustHideNotEnabled = ioProject.ProjectOptions.MustHideNotEnabled;
            project.ProjectOptions.MustHideCompleted = ioProject.ProjectOptions.MustHideCompleted;
            project.ViewOptions.DoShowPitch = ioProject.ViewOptions.DoShowPitch;
            project.ViewOptions.DoShowWaveform = ioProject.ViewOptions.DoShowWaveform;
            project.ViewOptions.DoShowSpectrum = ioProject.ViewOptions.DoShowSpectrum;

            project.SetVoicebank(new Voicebank(PathResolver.Current.TryGetDirectoryName(projectDir), ioProject.Voicebank));
            project.SetReclist(ReclistReader.Current.Read(ioProject.Reclist));
            project.SetReplacer(ReplacerReader.Current.Read(ioProject.Replacer, project.Reclist));

            foreach (var ioWavConfig in ioProject.WavConfigs)
            {
                if (ioWavConfig.File == null)
                {
                    continue;
                }
                if (!project.ProjectLinesByFilename.ContainsKey(ioWavConfig.File))
                {
                    project.AddProjectLine(ioWavConfig.File, new ProjectLine());
                }
                var projectLine = project.ProjectLinesByFilename[ioWavConfig.File];
                projectLine.SetPoints(ioWavConfig.Vowels, ioWavConfig.Consonants, ioWavConfig.Rests);
                projectLine.UpdateZones();
            }

            return project;
        }
    }


}
