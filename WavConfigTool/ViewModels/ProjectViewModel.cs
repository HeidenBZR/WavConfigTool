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
        public ObservableCollection<string> Reclists { get; private set; } = new ObservableCollection<string>() { "(Reclist)" };
        
        public string SelectedReclist
        {
            get => Project?.Reclist?.Name;
            set
            {
                Project.SetReclist(ReclistReader.Current.Read(value));
                ProjectDataChanged();
            }
        }

        public event SimpleHandler ProjectDataChanged;

        public ProjectViewModel()
        {
            ProjectDataChanged += delegate { };
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            GetReclistsAsync();
        }

        public ProjectViewModel(Project project)
        {
            ProjectDataChanged += delegate { };
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
            var list = new List<string>();
            string path = PathResolver.Current.Reclist();
            var files = Directory.GetFiles(path, "*.reclist");
            foreach (string filename in files)
            {
                var reclistName = Path.GetFileNameWithoutExtension(filename);
                var reclist = ReclistReader.Current.Read(reclistName);
                if (reclist != null && reclist.IsLoaded && !list.Contains(reclistName))
                    list.Add(reclistName);
            }
            Reclists = new ObservableCollection<string>(list);
        }

        private async void GetReclistsAsync()
        {
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
