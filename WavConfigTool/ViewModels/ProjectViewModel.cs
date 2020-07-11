using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WavConfigTool.Classes;
using WavConfigTool.ViewTools;
using WavConfigCore;
using WavConfigCore.Reader;
using WavConfigCore.Tools;
using System;

namespace WavConfigTool.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {

        public Project Project { get => _project; set { SetProject(value); } }
        public string VoicebankName => Project?.Voicebank?.GetFullName();
        public ObservableCollection<Reclist> Reclists { get; private set; } = new ObservableCollection<Reclist>();
        
        public Reclist SelectedReclist
        {
            get => Project?.Reclist;
            set
            {
                Project.SetReclist(value);
                ProjectDataChanged();
            }
        }

        public event SimpleHandler ProjectDataChanged = delegate { };

        public ProjectViewModel()
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            GetReclistsAsync();
        }

        public ProjectViewModel(Project project)
        {
            Project = project;
            // Остановить просчитывание в конструкторе
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            GetReclists();
        }

        #region private

        private Project _project = new Project();

        private void GetReclists()
        {
            var reclists = new List<Reclist>();
            var list = new List<string>();
            string path = PathResolver.Current.Reclist();
            var files = Directory.GetFiles(path, "*.reclist");
            foreach (string filename in files)
            {
                var reclist = ReadReclist(filename, list);
                if (reclist != null)
                    reclists.Add(reclist);
            }
            var testPath = Path.Combine(path, PathResolver.TEST_FOLDER);
            var filesTest = Directory.GetFiles(testPath, "*.reclist");
            foreach (string filename in filesTest)
            {
                var reclist = ReadReclist(filename, list, true);
                if (reclist != null)
                    reclists.Add(reclist);
            }

            App.MainDispatcher.Invoke(() =>
            {
                Reclists = new ObservableCollection<Reclist>(reclists);
                RaisePropertyChanged(nameof(Reclists));
            });
        }

        private Reclist ReadReclist(string filename, List<string> list, bool isTest = false)
        {
            var reclistName = Path.GetFileNameWithoutExtension(filename);
            var reclist = isTest ? ReclistReader.Current.ReadTest(reclistName) : ReclistReader.Current.Read(reclistName);
            if (reclist != null && reclist.IsLoaded && !list.Contains(reclistName))
            {
                list.Add(reclistName);
                return reclist;
            }
            return null;
        }

        private async void GetReclistsAsync()
        {
            Reclists.Clear();
            await Task.Run(() => ExceptionCatcher.Current.CatchOnAsyncCallback(GetReclists));
        }

        private void SetProject(Project project)
        {
            _project = project;
            if (_project == null)
                return;
            _project.ProjectChanged += () =>
            {
                RaisePropertiesChanged(
                    () => VoicebankName,
                    () => SelectedReclist
                );
            };
        }

        #endregion

        #region commands

        public ICommand ChangeVoicebankCommand => new OpenFileCommand((obj) =>
        {
            var location = PathResolver.Current.TryGetDirectoryName((string)obj);
            Project.SetVoicebank(new Voicebank(PathResolver.Current.TryGetDirectoryName(Settings.ProjectFile), location));
            ProjectDataChanged();
        },
        "Select voicebank samples",
        "Voicebank Files|*.wav;*.ini|*|*",
        param => (param != null));

        #endregion
    }
}
