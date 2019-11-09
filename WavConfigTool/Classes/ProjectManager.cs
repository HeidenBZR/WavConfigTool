using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Classes.Reader;
using WavConfigTool.Tools;
using WavConfigTool.ViewTools;

namespace WavConfigTool.Classes
{
    class ProjectManager
    {
        public Project Project;
        public void CheckForBackup()
        {
            if (File.Exists(Settings.TempProject))
                MustRecoverCommand.Execute(this);
        }

        public void CheckForLast()
        {
            if (File.Exists(Settings.ProjectFile))
            {
                Project = ProjectReader.Current.Read(Settings.ProjectFile);
            }
            Settings.IsUnsaved = false;
        }

        public void LoadProject()
        {
            Project = null;
            Settings.CheckPath();
            CheckForBackup();
            if (Project == null)
            {
                CheckForLast();
                if (Project == null)
                {
                    CreateProject();
                }
            }
            Project.SetOtoGenerator(new OtoGenerator(Project.Reclist, Project, Project.Replacer));
            Project.SaveMe += Save;
        }

        public void Open(string filename)
        {
            Reset();
            Settings.ProjectFile = filename;
            CheckForLast();
        }

        public void CreateProject()
        {
            var project = new Project();
            if (Project != null)
            {
                project.SetReclist(Project.Reclist);
                project.SetVoicebank(Project.Voicebank);
                project.SetOtoGenerator(Project.OtoGenerator);
            }
            Settings.IsUnsaved = false;
            this.Project = project;
            Settings.ProjectFile = "";
        }

        public void SaveAs(string filename)
        {
            if (Settings.ProjectFile == filename)
                return;
            Settings.ProjectFile = filename;
            Save();
        }

        public void Save()
        {
            try
            {
                if (Settings.ProjectFile == "")
                    throw new Exception();
                ProjectReader.Current.Write(Settings.ProjectFile, Project);
                if (File.Exists(Settings.TempProject))
                    File.Delete(Settings.TempProject);
            }
            catch
            {
                ProjectReader.Current.Write(Settings.TempProject, Project);
            }
        }

        public void Recover()
        {
            Project = ProjectReader.Current.Read(Settings.TempProject);
        }

        public MessageBoxConfirmationCommand MustRecoverCommand => new MessageBoxConfirmationCommand((obj) =>
        {
            Recover();
        }, "Recover",
           "Unsaved project found. Recover it?",
            (obj) => File.Exists(Settings.TempProject)
        );

        public void Reset()
        {
            Project = null;
        }

    }
}
