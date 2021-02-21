using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Tools;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

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
            string yaml = File.ReadAllText(filename, Encoding.UTF8);
            var ioProject = deserializer.Deserialize<IOProject>(yaml);
            return ioProject;
        }

        public void Write(string filename, Project project)
        {
            if (project == null)
                throw new Exception();
            WriteYaml(filename, GetIOProject(project));
        }

        public void WriteYaml(string filename, IOProject ioProject)
        {
            var yaml = serializer.Serialize(ioProject);
            if (yaml.Length == 0)
                throw new Exception("Failed to serialize project, project wasn't saved");
            else
                File.WriteAllText(filename, yaml, Encoding.UTF8);
        }

        private ISerializer serializer = new SerializerBuilder().Build();
        private IDeserializer deserializer = new DeserializerBuilder().Build();

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
                DoShowWaveform = project.ViewOptions.DoShowWaveform,
                SpectrumScale = project.ViewOptions.SpectrumScale,
                SpectrumShift = project.ViewOptions.SpectrumShift,
                SpectrumQualityX = project.ViewOptions.SpectrumQualityX,
                SpectrumQualityY = project.ViewOptions.SpectrumQualityY
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
            project.ViewOptions.SpectrumQualityY = ioProject.ViewOptions.SpectrumQualityY;
            project.ViewOptions.SpectrumQualityX = ioProject.ViewOptions.SpectrumQualityX;
            project.ViewOptions.SpectrumScale = ioProject.ViewOptions.SpectrumScale;
            project.ViewOptions.SpectrumShift = ioProject.ViewOptions.SpectrumShift;

            project.SetVoicebank(new Voicebank(PathResolver.Current.TryGetDirectoryName(projectDir), ioProject.Voicebank));
            var reclist = ReclistReader.Current.Read(ioProject.Reclist);
            if (!reclist.IsLoaded)
            {
                var testReclist = ReclistReader.Current.ReadTest(ioProject.Reclist);
                if (testReclist.IsLoaded)
                    reclist = testReclist;
            }
            project.SetReclist(reclist);
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
