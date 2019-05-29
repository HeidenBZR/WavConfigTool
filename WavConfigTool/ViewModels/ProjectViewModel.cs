using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using WavConfigTool.ViewTools;

namespace WavConfigTool.ViewModels
{
    public class ProjectViewModel : ViewModelBase
    {
        private Project _project = new Project();

        public Project Project
        {
            get => _project;
            set
            {
                _project = value;
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
        }
        public ObservableCollection<string> Reclists { get; private set; } = new ObservableCollection<string>() { "(Reclist)" };
        public string SelectedReclist
        {
            get => Project?.Reclist?.Name;
            set { Project.ChangeReclist(value); ProjectDataChanged(); }
        }
        public string VoicebankName
        {
            get => Project?.Voicebank?.Name;
        }
        public delegate void ProjectDataChangedHandler();
        public event ProjectDataChangedHandler ProjectDataChanged;

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
            GetReclistsAsync();
        }

        void GetReclists()
        {
            Reclists = new ObservableCollection<string>();
            string path = Settings.GetResoucesPath(@"WavConfigTool\WavSettings\");
            foreach (string filename in Directory.GetFiles(path, "*.wsettings"))
            {
                var reclist = Path.GetFileNameWithoutExtension(filename);
                if (!Reclists.Contains(reclist))
                    Reclists.Add(reclist);
            }
        }


        public ICommand ChangeVoicebankCommand => new OpenFileCommand((obj) =>
        {
            var location = Path.GetDirectoryName((string)obj);
            Project.ChangeVoicebank(location);
            ProjectDataChanged();
        },
        "Select voicebank samples",
        "Voicebank Files|*.wav;*.ini|*|*",
        param => (param != null));

        async void GetReclistsAsync()
        {
            await Task.Run(() => GetReclists());
        }

    }
}
