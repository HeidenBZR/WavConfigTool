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

        public string ReclistName { get => Project.Reclist.Name; }
        public string VoicebankName { get => Project.Voicebank.Name; }
        public string VoicebankImagePath { get => Project.Voicebank.ImagePath; }
        public BitmapImage VoicebankImage
        {
            get
            {
                if (VoicebankImagePath != "")
                    return new BitmapImage(new Uri(VoicebankImagePath));
                else
                    return null;
            }
        }

        public PagerViewModel PagerViewModel { get; set; }

        public int ConsonantAttack { get => Project.ConsonantAttack; set => Project.ConsonantAttack = value; }
        public int VowelAttack { get => Project.VowelAttack; set => Project.VowelAttack = value; }
        public int VowelSustain { get => Project.VowelSustain; set => Project.VowelSustain = value; }

        public string Prefix { get => Project.Prefix; set => Project.Prefix = value; }
        public string Suffix { get => Project.Suffix; set => Project.Suffix = value; }

        public double WavAmplitudeMultiplayer { get => Project.WavAmplitudeMultiplayer; set => Project.WavAmplitudeMultiplayer = value; }

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
            get => $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  [{Project.Voicebank.Name}] : {Project.Reclist.Name}";
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
            await Task.Run(() => LoadProject());
            if (!Project.IsLoaded)
            {
                CallProjectCommand.Execute(Project);
            }
            var wavControls = new ObservableCollection<WavControlViewModel>();
            for (int i = 0; i < Project.ProjectLines.Count; i++ )
                await Task.Run(() => { wavControls.Add(new WavControlViewModel(Project.ProjectLines[i]) { Number = i }); });

            PagerViewModel = new PagerViewModel(wavControls);
            await Task.Run(() => Parallel.ForEach(PagerViewModel.Collection, (model) => { model.Load(); }));
            IsLoading = false;
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

        public void GenerateOto()
        {

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

        public ICommand CallProjectCommand
        {
            get
            {
                return new DelegateCommonCommand((obj) =>
                {
                    ViewManager.CallProject(new ProjectViewModel(Project));
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
                return new DelegateCommand(delegate
                {
                    GenerateOto();
                }, delegate
                {
                    return Project != null && Project.IsLoaded;
                });
            }
        }

    }
}
