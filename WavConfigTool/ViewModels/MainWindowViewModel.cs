using DevExpress.Mvvm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using WavConfigTool.ViewTools;

namespace WavConfigTool.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {
        public static readonly Version Version = new Version(0, 1, 7, 0);
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
                        () => VoicebankImagePath,
                        () => PagerViewModel);

                    RaisePropertiesChanged(
                        () => ConsonantAttack,
                        () => VowelAttack,
                        () => VowelSustain,
                        () => Prefix,
                        () => Suffix);

                    RaisePropertiesChanged(
                        () => WavAmplitudeMultiplayer,
                        () => Title,
                        () => ReclistName,
                        () => VoicebankImage,
                        () => WavControlViewModels
                    );
                };
            }
        }

        public string ReclistName { get => Project == null || Project.Reclist == null ? null : Project.Reclist.Name; }
        public string VoicebankName { get => Project == null || Project.Voicebank == null ? null : Project.Voicebank.Name; }
        public string VoicebankImagePath { get => Project.Voicebank.ImagePath; }
        public BitmapImage VoicebankImage
        {
            get
            {
                if (Project != null && Project.Voicebank != null && Project.Voicebank.ImagePath != "")
                    return new BitmapImage(new Uri(Project.Voicebank.ImagePath));
                else
                    return null;
            }
        }

        public PagerViewModel PagerViewModel { get; set; }

        public int ConsonantAttack { get => Project == null ? 0 : Project.ConsonantAttack; set => Project.ConsonantAttack = value; }
        public int VowelAttack { get => Project == null ? 0 : Project.VowelAttack; set => Project.VowelAttack = value; }
        public int VowelSustain { get => Project == null ? 0 : Project.VowelSustain; set => Project.VowelSustain = value; }

        public string Prefix { get => Project == null ? "" : Project.Prefix; set => Project.Prefix = value; }
        public string Suffix { get => Project == null ? "" : Project.Suffix; set => Project.Suffix = value; }

        public double WavAmplitudeMultiplayer { get => Project == null ? 1 : Project.WavAmplitudeMultiplayer; set => Project.WavAmplitudeMultiplayer = value; }

        public PhonemeType Mode { get => Settings.Mode; set => Settings.Mode = value; }
        public string ModeSymbol { get => SymbolOfType(Mode); }

        public PhonemeType ModeC { get => PhonemeType.Consonant; }
        public PhonemeType ModeV { get => PhonemeType.Vowel; }
        public PhonemeType ModeR { get => PhonemeType.Rest; }

        public string ModeCSymbol { get => SymbolOfType(ModeC); }
        public string ModeVSymbol { get => SymbolOfType(ModeV); }
        public string ModeRSymbol { get => SymbolOfType(ModeR); }

        public double ToolsPanelHeight { get; set; } = 80;
        public const int TOOLS_PANEL_OPENED_HEIGHT = 80;
        private bool _isToolsPanelShown = true;
        public bool IsToolsPanelShown { get => _isToolsPanelShown; set { _isToolsPanelShown = value; RaisePropertyChanged(() => IsToolsPanelShown);  } }

        public bool IsLoading { get; set; } = false;
        public bool IsNotLoading { get => !IsLoading; }

        public string ProjectSavedString { get => Settings.IsUnsaved ? "*" : ""; }
        public string Title
        {
            get
            {
                if (Project == null)
                    return $"WavConfig v.{Version.ToString()} - {ProjectSavedString}";
                if (Project.Voicebank == null)
                    return $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  : {Project.Reclist.Name}";
                if (Project.Reclist == null)
                    return $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  [{Project.Voicebank.Name}]";
                if (PagerViewModel == null || PagerViewModel.PagesTotal == 0)
                    return $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  [{Project.Voicebank.Name}] : {Project.Reclist.Name}";
                return $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  [{Project.Voicebank.Name}] : {Project.Reclist.Name} | " +
                    $"Page {PagerViewModel.CurrentPage + 1}/{PagerViewModel.PagesTotal}";
            }
        }

        public string SymbolOfType(PhonemeType type)
        {
            return type.ToString().Substring(0, 1);
        }

        public ObservableCollection<WavControlViewModel> WavControlViewModels { get => PagerViewModel.Collection; }
        public ObservableCollection<WavControlViewModel> WavControlViewModelsPage { get => PagerViewModel.PageContent;  }


        //Point PrevMousePosition;
        private Project _project;

        public MainWindowViewModel()
        {
            // Остановить просчитывание в конструкторе
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                return;
            // Загрузить бэкап/последний/новый проект
            LoadProjectAsync();
        }

        async void LoadProjectAsync()
        {
            IsLoading = true;
            RaisePropertyChanged(() => IsLoading);
            await Task.Run(() => LoadProject());
            if (!Project.IsLoaded)
            {
                CallProjectCommand.Execute(Project);
            }
            var wavControls = new ObservableCollection<WavControlViewModel>();
            for (int i = 0; i < Project.ProjectLines.Count; i++ )
                await Task.Run(() => { wavControls.Add(new WavControlViewModel(Project.ProjectLines[i]) { Number = i }); });

            PagerViewModel = new PagerViewModel(wavControls);
            PagerViewModel.PagerChanged += delegate { RaisePropertyChanged(() => Title); };
            await Task.Run(() => Parallel.ForEach(PagerViewModel.Collection, (model) => { model.Load(); }));
            IsLoading = false;
            RaisePropertyChanged(() => IsLoading);
        }

        void LoadProject()
        {
            Settings.CheckPath();
            Project = Project.OpenBackup();
            if (Project is null)
            {
                Project = Project.OpenLast();
                if (Project is null)
                    Project = new Project();
            }
        }

        public void ResetProject()
        {
            PagerViewModel.Clear();
            Project = null;
            IsLoading = true;
        }

        public ICommand SetMode
        {
            get
            {
                return new DelegateCommonCommand((obj) =>
                {
                    Mode = (PhonemeType)obj;
                }, param => (param != null));
            }
        }

        public ICommand NewProjectCommand
        {
            get
            {
                return new SaveFileCommand((obj) =>
                {
                    string filename = (string)obj;
                    if (filename.Length > 0)
                    {
                        Settings.ProjectFile = (string)obj;
                        ResetProject();
                        LoadProjectAsync();
                    }
                },
                "Save New Project",
                "WavConfig Project Files|*.wconfig|*|*",
                param => (param != null),
                "voicebank");
            }
        }

        public ICommand OpenProjectCommand
        {
            get
            {
                return new OpenFileCommand((obj) =>
                {
                    string filename = (string)obj;
                    if (filename.Length > 0)
                    {
                        Settings.ProjectFile = (string)obj;
                        ResetProject();
                        LoadProjectAsync();
                    }
                },
                "Save New Project",
                "WavConfig Project Files|*.wconfig|*|*",
                param => (param != null),
                "voicebank");
            }
        }
        
        public ICommand CallProjectCommand
        {
            get
            {
                return new DelegateCommonCommand((obj) =>
                {
                    var projectViewModel = new ProjectViewModel(Project);
                    projectViewModel.ProjectDataChanded += delegate { LoadProjectAsync(); };
                    ViewManager.CallProject(projectViewModel);
                }, (param) => (Project != null));
            }
        }

        public ICommand ToggleToolsPanelCommand
        {
            get
            {
                return new DelegateCommand(delegate
                {
                    IsToolsPanelShown = !IsToolsPanelShown;
                //    if (ToolsPanelHeight == TOOLS_PANEL_OPENED_HEIGHT)
                //        ToolsPanelHeight = 0;
                //    else
                //        ToolsPanelHeight = TOOLS_PANEL_OPENED_HEIGHT;
                }, delegate
                {
                    return Project != null && Project.IsLoaded;
                });
            }
        }

        public ICommand GenerateOtoCommand
        {
            get
            {
                return new SaveFileCommand((obj) =>
                {
                    string filename = (string)obj;
                    if (filename.Length > 0)
                    {
                        Project.GenerateOto(filename);
                    }
                },
                "Save Oto",
                "Oto Files|*oto*.ini|*|*",
                delegate
                {
                    return Project != null && Project.IsLoaded;
                },
                "oto");
            }
        }

    }
}
