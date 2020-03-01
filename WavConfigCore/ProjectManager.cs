using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigCore.Reader;
using WavConfigCore.Tools;

namespace WavConfigCore
{
    public class ProjectManager
    {
        #region singleton base

        private static ProjectManager current;
        private ProjectManager()
        {
            WatchForBackup();
        }

        public static ProjectManager Current
        {
            get
            {
                if (current == null)
                {
                    current = new ProjectManager();
                }
                return current;
            }
        }

        #endregion

        public Project Project;

        public void LoadProject(string lastProject)
        {
            Reset();
            CheckForLast(lastProject);
            if (Project == null)
            {
                CreateProject();
                AfterProjectLoaded(lastProject);
            }
        }

        public void Open(string filename)
        {
            Reset();
            CheckForLast(filename);
        }

        public void CreateProject(string filename = "")
        {
            var project = new Project(filename);
            if (Project != null)
            {
                project.SetReclist(Project.Reclist);
            }
            var directoryName = filename != "" ? Path.GetDirectoryName(filename) : "";
            project.SetVoicebank(new Voicebank(directoryName, ""));
            project.SetReplacer(new Replacer());
            if (project.Voicebank != null && project.Voicebank.IsLoaded)
            {
                project.Suffix = project.Voicebank.Subfolder;
            }
            Project = project;
        }

        public void SaveAs(string filename)
        {
            if (Project.Voicebank != null)
                Project.Voicebank.UpdateLocations(filename);
            Save(filename);
        }

        public void Save(string projectPath)
        {
            ProjectReader.Current.Write(projectPath, Project);
        }

        public void Reset()
        {
            Project = null;
        }

        #region private

        private void WatchForBackup()
        {
            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += new EventHandler(MakeBackup);
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Start();
        }

        private void MakeBackup(object sender, EventArgs e)
        {
            if (Project != null && Project.IsLoaded && Project.IsChangedAfterBackup)
            {
                Reader.ProjectReader.Current.Write(PathResolver.Current.Backup(), Project);
                Project.HandleBackupSaved();
            }
            var files = Directory.GetFiles(PathResolver.Current.Backup(onlyFolder: true), "backup*" + PathResolver.PROJECT_EXT).ToList();
            files.Sort();
            files.Reverse();
            for (var i = 10; i < files.Count; i++)
            {
                File.Delete(files[i]);
            }
        }

        private void CheckForLast(string lastProject)
        {
            if (File.Exists(lastProject))
            {
                Project = ProjectReader.Current.Read(lastProject);
                AfterProjectLoaded(lastProject);
            }
        }

        private void AfterProjectLoaded(string projectDir)
        {
            Project.SaveMe += () => Save(projectDir);
        }

        #endregion
    }
}
