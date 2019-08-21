using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace WavConfigTool.Classes.IO
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

        public Project Read(string name)
        {
            var project = ReadYaml(name);
            return project ?? new Project();
        }

        private Project ReadYaml(string name)
        {
            throw new NotImplementedException();
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
            ioProject.Voicebank = project.Voicebank.Location;
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
                wavConfigsList.Add(ioWavConfig);
            }
            ioProject.WavConfigs = wavConfigsList.ToArray();

            return ioProject;
        }

        private Project GetProject(IOProject ioProject)
        {
            throw new NotImplementedException();
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
        public string File;
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
