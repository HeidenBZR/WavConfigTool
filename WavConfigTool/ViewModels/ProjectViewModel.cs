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
            var reclists = WavConfigCore.WavConfigCore.Current.GetReclists();

            App.MainDispatcher.Invoke(() =>
            {
                Reclists = new ObservableCollection<Reclist>(reclists);
                RaisePropertyChanged(nameof(Reclists));
            });
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
