using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.Classes.Reader;
using WavConfigTool.Tools;
using WavConfigTool.ViewTools;

namespace WavConfigTool.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {
        public static readonly Version Version = new Version(0, 2, 0, 0);
        public Project Project => ProjectManager.Project;
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
        public PagerViewModel WavControlsPagerViewModel { get; set; }
        public PagerViewModel OtoPagerViewModel { get; set; }
        public ProjectManager ProjectManager { get; private set; } = new ProjectManager();

        public int ConsonantAttack { get => Project == null ? 0 : Project.ConsonantAttack; set => Project.ConsonantAttack = value; }
        public int VowelAttack { get => Project == null ? 0 : Project.VowelAttack; set => Project.VowelAttack = value; }
        public int RestAttack { get => Project == null ? 0 : Project.RestAttack; set => Project.RestAttack = value; }
        public int VowelDecay { get => Project == null ? 0 : Project.VowelDecay; set => Project.VowelDecay = value; }

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
        public bool IsToolsPanelShown { get => _isToolsPanelShown; set { _isToolsPanelShown = value; RaisePropertyChanged(() => IsToolsPanelShown); } }

        public bool IsLoading { get; set; } = false;
        public bool IsNotLoading { get => !IsLoading; }
        public bool IsOtoPreviewMode { get; set; } = false;

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

        public static double ControlHeight { get; set; } = 100;

        public string SymbolOfType(PhonemeType type)
        {
            return type.ToString().Substring(0, 1);
        }

        public ObservableCollection<WavControlBaseViewModel> WavControlViewModels { get => PagerViewModel.Collection; }
        public ObservableCollection<WavControlBaseViewModel> WavControlViewModelsPage { get => PagerViewModel.PageContent; }



        public MainWindowViewModel()
        {
        }

        void ConvertWavsettingsToReclist(string name)
        {
            var reclist = WavSettingsReader.Current.Read(name);
            ReclistReader.Current.Write(name, reclist);
        }

        async void LoadProjectAsync()
        {
            IsLoading = true;
            RaisePropertyChanged(() => IsLoading);
            if (Project != null && Project.IsLoaded)
            {
                var wavControls = new ObservableCollection<WavControlBaseViewModel>();
                for (int i = 0; i < Project.ProjectLines.Count; i++)
                {
                    if (Project.ProjectLines[i].IsEnabled)
                        await Task.Run(() => { wavControls.Add(CreateWavControl(i)); });
                }

                WavControlsPagerViewModel = new PagerViewModel(wavControls);
                PagerViewModel = WavControlsPagerViewModel;
                PagerViewModel.PagerChanged += delegate { RaisePropertyChanged(() => Title); };
                Project.BeforeSave += () => { WriteProjectOptions(); };
                await Task.Run(() => Parallel.ForEach(PagerViewModel.Collection, (model) => { (model as WavControlViewModel).Load(); }));
            }
            IsLoading = false;
            Refresh();
        }

        private void WriteProjectOptions()
        {
            PagerViewModel.WriteProjectOptions(Project.ProjectOptions);
            // дозаписать свои если есть
        }

        WavControlViewModel CreateWavControl(int i)
        {
            var projectLine = Project.ProjectLines[i];
            var control = new WavControlViewModel(projectLine) { Number = i };
            control.OnOtoMode += delegate
            {
                SetOtoMode(control);
            };
            control.RegenerateOtoRequest += delegate
            {
                if (IsOtoPreviewMode)
                {
                    OtoGenerator.Current.Generate(projectLine);
                    OtoPagerViewModel.UpdateOtoPreviewControls(control.GenerateOtoPreview());
                    RaisePropertyChanged(() => PagerViewModel);
                }
            };
            return control;
        }

        void SetOtoMode(WavControlViewModel wavControl)
        {
            if (IsOtoPreviewMode)
            {
                SetWavConfigMode.Execute(null);
            }
            else
            {
                wavControl.IsOtoBase = true;
                OtoGenerator.Current.Generate(wavControl.ProjectLine);
                OtoPagerViewModel = new PagerViewModel(wavControl.GenerateOtoPreview());
                OtoPagerViewModel.Base = wavControl;
                OtoPagerViewModel.OtoMode();
                PagerViewModel = OtoPagerViewModel;
                IsOtoPreviewMode = true;
                Refresh();
            }
        }

        public void Refresh()
        {
            if (Project == null)
                return;
            RaisePropertiesChanged(
                () => VoicebankName,
                () => VoicebankImagePath,
                () => PagerViewModel);

            RaisePropertiesChanged(
                () => ConsonantAttack,
                () => VowelAttack,
                () => VowelDecay,
                () => Prefix,
                () => Suffix);

            RaisePropertiesChanged(
                () => WavAmplitudeMultiplayer,
                () => Title,
                () => ReclistName,
                () => VoicebankImage,
                () => WavControlViewModels
            );
            RaisePropertyChanged(() => IsLoading);
        }


        public void ResetProject()
        {
            if (PagerViewModel != null)
                PagerViewModel.Clear();
            ProjectManager.Reset();
            IsLoading = true;
            Refresh();
        }

        public ICommand SetMode => new DelegateCommonCommand((obj) =>
        {
            Mode = (PhonemeType)obj;
        }, param => (param != null));

        public ICommand NewProjectCommand => new SaveFileCommand((obj) =>
        {
            string filename = (string)obj;
            if (filename.Length > 0)
            {
                ResetProject();
                ProjectManager.CreateProject();
                CallProjectCommand.Execute(this);
                ProjectManager.SaveAs(filename);
                LoadProjectAsync();
            }
        },
        "Save New Project",
        "WavConfig Project Files|*.wcp|*|*",
        param => true,
        "voicebank");

        public ICommand OpenProjectCommand => new OpenFileCommand((obj) =>
        {
            string filename = (string)obj;
            if (filename.Length > 0)
            {
                ResetProject();
                ProjectManager.Open((string)obj);
                CallProjectCommand.Execute(this);
                LoadProjectAsync();
            }
        },
        "Save New Project",
        "WavConfig Project Files|*.wcp|*|*",
        param => true,
        "voicebank");

        public ProjectViewModel ProjectViewModel { get; set; }
        public ICommand CallProjectCommand => new DelegateCommonCommand((obj) =>
        {
            if (ProjectViewModel != null)
                return;
            ProjectViewModel = new ProjectViewModel(Project);
            ProjectViewModel.ProjectDataChanged += delegate { LoadProjectAsync(); };
            ViewManager.CallProject(ProjectViewModel);
            ProjectViewModel = null;
        }, (param) => (Project != null));

        public ICommand ToggleToolsPanelCommand => new DelegateCommand(delegate
        {
            IsToolsPanelShown = !IsToolsPanelShown;
        }, () => Project != null && Project.IsLoaded);

        public ICommand GenerateOtoCommand => new SaveFileCommand((obj) =>
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

        public ICommand LoadedCommand => new DelegateCommand(() =>
        {
            ProjectManager.LoadProject();
            LoadProjectAsync();
            Refresh();
        }, () => true);

        public ICommand ReloadProjectCommand => new DelegateCommand(() =>
        {
            ProjectManager.LoadProject();
            LoadProjectAsync();
            Refresh();
        }, () => !IsLoading);

        public ICommand SetWavConfigMode => new DelegateCommand(() =>
        {
            PagerViewModel = WavControlsPagerViewModel;
            IsOtoPreviewMode = false;
            Refresh();
        }, () => IsOtoPreviewMode);

    }
}
