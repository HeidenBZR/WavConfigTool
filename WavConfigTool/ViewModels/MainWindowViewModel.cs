﻿using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WavConfigTool.Classes;
using WavConfigTool.ViewTools;
using WavConfigCore;
using WavConfigCore.Reader;

namespace WavConfigTool.ViewModels
{

    class MainWindowViewModel : ViewModelBase
    {
        public static readonly Version Version = new Version(0, 2, 0, 0);

        public int AlphaVersion => 28;

        public ProjectViewModel ProjectViewModel { get; set; }
        public Project Project => ProjectManager.Project;
        public string ReclistName => Project != null && Project.Voicebank != null && Project.IsLoaded ? Project.Reclist.Name : null;
        public string VoicebankName => Project != null && Project.Voicebank != null && Project.IsLoaded ? Project.Voicebank.Name : null;
        public string VoicebankImagePath => Project != null && Project.Voicebank != null && Project.IsLoaded ? Project.Voicebank.ImagePath : null;
        public string VoicebankSubfolder => Project != null && Project.Voicebank != null && Project.IsLoaded ? Project.Voicebank.Subfolder : null;
        public BitmapImage VoicebankImage => GetVoicebankImage();

        public PagerViewModel PagerViewModel { get; set; }
        public PagerViewModel WavControlsPagerViewModel { get; set; }
        public PagerViewModel OtoPagerViewModel { get; set; }
        public ProjectManager ProjectManager { get; private set; } = ProjectManager.Current;
        public OtoGenerator OtoGenerator { get; private set; }

        public int ConsonantAttack { get => Project == null ? 0 : Project.ConsonantAttack; set => Project.ConsonantAttack = value; }
        public int VowelAttack { get => Project == null ? 0 : Project.VowelAttack; set => Project.VowelAttack = value; }
        public int RestAttack { get => Project == null ? 0 : Project.RestAttack; set => Project.RestAttack = value; }
        public int VowelDecay { get => Project == null ? 0 : Project.VowelDecay; set => Project.VowelDecay = value; }

        public bool MustHideNotEnabled
        {
            get => Project != null && Project.ProjectOptions.MustHideNotEnabled;
            set { Project.ProjectOptions.MustHideNotEnabled = value; PagerViewModel.RequestUpdateCollection(); }
        }
        public bool MustHideCompleted
        {
            get => Project != null && Project.ProjectOptions.MustHideCompleted;
            set { Project.ProjectOptions.MustHideCompleted = value; PagerViewModel.RequestUpdateCollection(); }
        }

        public string Prefix { get => Project?.Prefix; set => Project.Prefix = value; }
        public string Suffix { get => Project?.Suffix; set => Project.Suffix = value; }

        public string WavPrefix
        {
            get => Project?.WavPrefix;
            set
            {
                Project.WavPrefix = value;
                ReloadProjectCommand.Execute(0);
            }
        }
        public string WavSuffix
        {
            get => Project?.WavSuffix;
            set
            {
                Project.WavSuffix = value;
                ReloadProjectCommand.Execute(0);
            }
        }

        public double UserScaleY
        {
            get => Settings.UserScaleY;
            set { Project.UserScaleY = value; ReloadProjectCommand.Execute(0); }
        }
        public double UserScaleX
        {
            get => Settings.UserScaleX;
            set { Project.UserScaleX = value; ReloadProjectCommand.Execute(0); }
        }

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
        public bool IsToolsPanelShown { get => _isToolsPanelShown; set { _isToolsPanelShown = value; RaisePropertyChanged(() => IsToolsPanelShown); } }

        public bool IsLoading { get; set; } = false;
        public bool IsNotLoading { get => !IsLoading; }
        public bool IsOtoPreviewMode { get; set; } = false;

        public static bool IsDebug { get; set; } = false;

        public string Title => GetTitle();

        public ObservableCollection<WavControlBaseViewModel> WavControlViewModels { get => PagerViewModel.Collection; }
        public ObservableCollection<WavControlBaseViewModel> WavControlViewModelsPage { get => PagerViewModel.PageContent; }

        public MainWindowViewModel()
        {
#if DEBUG
            IsDebug = true;
#endif
        }

        #region private

        private bool _isToolsPanelShown = true;

        private async void LoadProjectAsync()
        {
            IsLoading = true;
            RaisePropertyChanged(() => IsLoading);
            if (Project != null && Project.IsLoaded)
            {
                var wavControls = new List<WavControlBaseViewModel>();
                for (int i = 0; i < Project.ProjectLines.Count; i++)
                {
                    await Task.Run(() => ExceptionCatcher.Current.CatchOnAsyncCallback(() =>
                        wavControls.Add(CreateWavControl(i))));
                }

                WavControlsPagerViewModel = new PagerViewModel(wavControls);
                PagerViewModel = WavControlsPagerViewModel;
                PagerViewModel.PagerChanged += delegate { RaisePropertyChanged(() => Title); };
                Project.BeforeSave += () => { WriteProjectOptions(); };
                foreach (var control in PagerViewModel.Collection)
                {
                    control.Ready();
                }
                PagerViewModel.ReadProjectOption(Project.ProjectOptions);
                PagerViewModel.RequestUpdateCollection();
                PagerViewModel.WaitForPageLoadedAndLoadRest();
                OtoGenerator = new OtoGenerator(Project);
            }
            IsLoading = false;
            App.MainDispatcher.Invoke(() =>
            {
                Refresh();
            });
        }

        private void WriteProjectOptions()
        {
            PagerViewModel.WriteProjectOptions(Project.ProjectOptions);
            // дозаписать свои если есть
        }

        private WavControlViewModel CreateWavControl(int i)
        {
            var projectLine = Project.ProjectLines[i];
            var sampleName = Project.Voicebank.GetSamplePath(projectLine.Recline.Name, Project.WavPrefix, Project.WavSuffix);
            var hash = $"{Project.Voicebank.Name}_{Project.Reclist.Name}_{Settings.UserScaleX}x{Settings.UserScaleY}_{sampleName}"; //.GetHashCode();
            var control = new WavControlViewModel(projectLine, sampleName, hash) { Number = i };
            control.OnOtoMode += delegate
            {
                SetOtoMode(control);
            };
            control.OnGenerateOtoRequested += delegate
            {
                if (IsOtoPreviewMode)
                {
                    Project.ResetOto();
                    projectLine.Recline.ResetOto();
                    OtoGenerator.GenerateFromProjectLine(projectLine);
                    OtoPagerViewModel.UpdateOtoPreviewControls(control.GenerateOtoPreview());
                    RaisePropertyChanged(() => PagerViewModel);
                }
            };
            control.OnChangePhonemeModeRequested += delegate (PhonemeType type)
            {
                SetPhonemeModeCommand.Execute(type);
            };
            return control;
        }

        private void Refresh()
        {
            if (Project == null)
                return;
            RaisePropertiesChanged(
                () => VoicebankName,
                () => VoicebankSubfolder,
                () => VoicebankImagePath,
                () => PagerViewModel,
                () => WavControlViewModels
            );

            RaisePropertiesChanged(
                () => ConsonantAttack,
                () => VowelAttack,
                () => RestAttack,
                () => VowelDecay
            );

            RaisePropertiesChanged(
                () => WavPrefix,
                () => WavSuffix,
                () => Prefix,
                () => Suffix
            );

            RaisePropertiesChanged(
                () => UserScaleY,
                () => UserScaleX,
                () => Title,
                () => ReclistName,
                () => VoicebankImage
            );

            RaisePropertiesChanged(
                () => MustHideCompleted,
                () => MustHideNotEnabled
            );
            RaisePropertyChanged(() => IsLoading);
        }


        private void ResetProject()
        {
            if (PagerViewModel != null)
                PagerViewModel.Clear();
            ProjectManager.Reset();
            IsLoading = true;
            Refresh();
        }

        private string GetTitle()
        {
            var projectFileName = Project != null && Project.IsLoaded ? " " + Path.GetFileName(Settings.ProjectFile) : "";
            var alphaString = $"(Alpha v.{AlphaVersion})";
            if (Project == null)
                return $"WavConfig v.{Version.ToString()} {alphaString}{projectFileName}";
            if (PagerViewModel == null || PagerViewModel.PagesTotal == 0)
                return $"WavConfig v.{Version.ToString()} {alphaString}{projectFileName}  [{Project.Voicebank.Name}] : {Project.Reclist.Name}";
            return $"WavConfig v.{Version.ToString()} {alphaString}{projectFileName} [{Project.Voicebank.Name}] : {Project.Reclist.Name} | " +
                $"Page {PagerViewModel.CurrentPage + 1}/{PagerViewModel.PagesTotal}";
        }

        private void SetOtoMode(WavControlViewModel wavControl)
        {
            if (IsOtoPreviewMode)
            {
                SetWavConfigMode.Execute(null);
            }
            else
            {
                wavControl.IsOtoBase = true;
                Project.ResetOto();
                wavControl.ProjectLine.Recline.ResetOto();
                OtoGenerator.GenerateFromProjectLine(wavControl.ProjectLine);
                OtoPagerViewModel = new PagerViewModel(wavControl.GenerateOtoPreview())
                {
                    Base = wavControl
                };
                OtoPagerViewModel.OtoMode();
                PagerViewModel = OtoPagerViewModel;
                IsOtoPreviewMode = true;
                Refresh();
            }
        }

        private BitmapImage GetVoicebankImage()
        {
            if (Project != null && Project.Voicebank != null && Project.Voicebank.ImagePath != "" && File.Exists(Project.Voicebank.ImagePath))
                return new BitmapImage(new Uri(Project.Voicebank.ImagePath));
            else
                return null;
        }

        private string SymbolOfType(PhonemeType type)
        {
            return type.ToString().Substring(0, 1);
        }

        #endregion

        #region Commands

        public ICommand SetPhonemeModeCommand => new DelegateCommonCommand((obj) =>
        {
            Mode = (PhonemeType)obj;
        }, param => (param != null));

        public ICommand NewProjectCommand => new SaveFileCommand((obj) =>
        {
            string filename = (string)obj;
            if (filename.Length > 0)
            {
                ResetProject();
                Settings.ProjectFile = filename;
                ProjectManager.CreateProject(filename);
                CallProjectCommand.Execute(this);
                ProjectManager.Save(Settings.ProjectFile);
                LoadProjectAsync();
            }
        },
        "Save New Project",
        "WavConfig Project Files|*.wcp|*|*",
        param => true,
        "voicebank");

        public ICommand SaveProjectAsNewCommand => new SaveFileCommand((obj) =>
        {
            string filename = (string)obj;
            if (filename.Length > 0)
            {
                ProjectManager.SaveAs((string)obj);
                Refresh();
            }
        },
        "Save Project As",
        "WavConfig Project Files|*.wcp|*|*",
        param => true,
        "voicebank");

        public ICommand OpenProjectCommand => new OpenFileCommand((obj) =>
        {
            string filename = (string)obj;
            if (filename.Length > 0)
            {
                ResetProject();
                Settings.ProjectFile = filename;
                ProjectManager.Open(Settings.ProjectFile);
                CallProjectCommand.Execute(this);
                LoadProjectAsync();
            }
        },
        "Save New Project",
        "WavConfig Project Files|*.wcp|*|*",
        param => true,
        "voicebank");


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
                OtoGenerator.GenerateAllAndSave(filename);
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
            ProjectManager.LoadProject(Settings.ProjectFile);
            LoadProjectAsync();
            Refresh();
        }, () => true);

        public ICommand ReloadProjectCommand => new DelegateCommand(() =>
        {
            ProjectManager.LoadProject(Settings.ProjectFile);
            LoadProjectAsync();
            Refresh();
            PagerViewModel.RequestUpdateCollection();
        }, () => !IsLoading);

        public ICommand SetWavConfigMode => new DelegateCommand(() =>
        {
            PagerViewModel = WavControlsPagerViewModel;
            IsOtoPreviewMode = false;
            Refresh();
        }, () => IsOtoPreviewMode);

        public ICommand DebugCommand => new DelegateCommand(() =>
        {
            foreach (var projectLine in Project.ProjectLines)
            {
                foreach (var type in new[] { PhonemeType.Consonant, PhonemeType.Rest, PhonemeType.Vowel })
                {
                    var points = projectLine.PointsOfType(type, false);
                    for (var i = 0; i < points.Count; i++)
                    {
                        points[i] = (int)(points[i] * 1.4 / WaveForm.X_SCALE_ERROR);
                    }
                }
            }
            Project.FireSaveMe();
            ReloadProjectCommand.Execute(0);
        }, () => IsDebug);

        #endregion
    }
}
