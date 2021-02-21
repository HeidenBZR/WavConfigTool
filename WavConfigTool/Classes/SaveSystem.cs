using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WavConfigCore;

namespace WavConfigTool.Classes
{
    class SaveSystem
    {
        public event SimpleHandler OnSaved = delegate { };

        public SaveSystem(ProjectManager projectManager)
        {
            this.projectManager = projectManager;
            App.Current.Exit += HandleApplicationExit; ;
        }

        public void Start()
        {
            scheduler.Schedule(SAVE_DELAY_SEC, Save);

            System.Windows.Threading.DispatcherTimer timer = new System.Windows.Threading.DispatcherTimer();

            timer.Tick += delegate {
                projectManager.SaveBackup();
            };
            timer.Interval = new TimeSpan(0, 1, 0);
            timer.Start();
        }

        public void Stop()
        {
            scheduler.CancellAll();
        }

        public void SaveImmediately()
        {
            ExceptionCatcher.Current.CatchOnAction(() => projectManager.Save(Settings.ProjectFile), "Failed to save project");
            OnSaved();
        }

        private const int SAVE_DELAY_SEC = 15;
        private readonly ProjectManager projectManager;
        private readonly Scheduler scheduler = new Scheduler();

        private void Save()
        {
            if (projectManager.Project != null && projectManager.Project.IsLoaded && projectManager.Project.IsChangedAfterBackup)
            {
                try
                {
                    projectManager.Save(Settings.ProjectFile);
                    projectManager.SaveBackup();
                    OnSaved();
                }
                catch (Exception ex)
                {
                    ExceptionCatcher.Current.Catch(ex, "Failed to save project");
                }
            }
        }

        private void HandleApplicationExit(object sender, System.Windows.ExitEventArgs e)
        {
            Stop();
            SaveImmediately();
        }
    }
}
