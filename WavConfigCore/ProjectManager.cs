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
            LoadIfExists(lastProject);
        }

        public void Open(string filename)
        {
            Reset();
            LoadIfExists(filename);
        }

        public void CreateProject(string filename = "")
        {
            var project = new Project(filename);
            if (Project != null)
            {
                project.SetReclist(Project.Reclist);
            }
            project.SetVoicebank(new Voicebank(PathResolver.Current.TryGetDirectoryName(filename), ""));
            project.SetReplacer(new Replacer());
            if (project.Voicebank != null && project.Voicebank.IsLoaded)
            {
                project.Suffix = project.Voicebank.Subfolder;
            }

            if (project.Voicebank.Type != "")
            {
                var reclist = ReclistReader.Current.Read(project.Voicebank.Type);
                if (reclist.IsLoaded)
                {
                    project.SetReclist(reclist);
                }
            }

            Project = project;
            if (filename != "")
                AfterProjectLoaded(filename);
        }

        public void SaveAs(string filename)
        {
            if (Project.Voicebank != null)
            {
                var dir = PathResolver.Current.TryGetDirectoryName(filename, "");
                Project.Voicebank.UpdateLocations(dir);
            }
            Save(filename);
        }

        public void Save(string projectPath)
        {
            if (Project == null || !Project.IsLoaded)
                return;
            Project.FireBeforeSave();
            Console.WriteLine("ProjectManager: Save " + DateTime.Now.ToString());
            ProjectReader.Current.Write(projectPath, Project);
        }

        public void SaveBackup()
        {
            if (Project != null && Project.IsLoaded && Project.IsChangedAfterBackup)
            {
                Console.WriteLine("ProjectManager: SaveBackup " + DateTime.Now.ToString());
                Project.FireBeforeSave();
                var filename = PathResolver.Current.Backup();
                if (File.Exists(filename))
                    return;
                ProjectReader.Current.Write(filename, Project);
                Project.HandleBackupSaved();

                var files = Directory.GetFiles(PathResolver.Current.Backup(onlyFolder: true), "backup*" + PathResolver.PROJECT_EXT).ToList();
                files.Sort();
                files.Reverse();
                for (var i = BACKUP_COUNT; i < files.Count; i++)
                {
                    File.Delete(files[i]);
                }
            }
        }

        public void Reset()
        {
            Project = null;
        }

        #region private

        private const int BACKUP_COUNT = 50;

        private void LoadIfExists(string filename)
        {
            if (File.Exists(filename))
            {
                Project = ProjectReader.Current.Read(filename);
                AfterProjectLoaded(filename);
            }
        }

        private void AfterProjectLoaded(string projectDir)
        {

        }

        #endregion
    }
}
