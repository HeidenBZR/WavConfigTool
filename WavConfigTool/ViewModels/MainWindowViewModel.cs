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
using System.Diagnostics;
using WavConfigCore.Tools;
using System.Reflection;
using System.Drawing;
using WavConfigTool.UserControls;

namespace WavConfigTool.ViewModels
{
    internal class MainWindowViewModel : ViewModelBase
    {

        public ProjectViewModel ProjectViewModel { get; set; }
        public string Version => "v.0.2.3.6";
        public Project Project => ProjectManager.Project;
        public string ReclistName           => IsProjectReady && Project?.Voicebank != null ? Project.Reclist.Name : null;
        public string VoicebankName         => IsProjectReady && Project?.Voicebank != null ? Project.Voicebank.Name : null;
        public string VoicebankImagePath    => IsProjectReady && Project?.Voicebank != null ? Project.Voicebank.ImagePath : null;
        public string VoicebankSubfolder    => IsProjectReady && Project?.Voicebank != null ? Project.Voicebank.Subfolder : null;
        public BitmapImage VoicebankImage => GetVoicebankImage();

        public bool IsProjectReady => Project != null;

        public PagerViewModel PagerViewModel { get; set; }
        public PagerViewModel WavControlsPagerViewModel { get; set; }
        public PagerViewModel OtoPagerViewModel { get; set; }
        public ProjectManager ProjectManager => ProjectManager.Current;
        public GotoUserControlViewModel GotoUserControlViewModel { get; set; }
        public OtoGenerator OtoGenerator { get; private set; }
        public OremoPackGenerator OremoPackGenerator { get; set; } = new OremoPackGenerator();
        public WavPlayer WavPlayer = new WavPlayer();
        public ImagesLibrary ImagesLibrary = new ImagesLibrary();
        public SaveSystem SaveSystem;

        public int SpectrogramShift         { get => ViewOptions.SpectrumShift; set { ViewOptions.SpectrumShift = value; FireProjectChanged(); } }
        public double SpectrogramScale      { get => ViewOptions.SpectrumScale; set { ViewOptions.SpectrumScale = value; FireProjectChanged(); } }
        public int SpectrogramQualityX      { get => ViewOptions.SpectrumQualityX; set { ViewOptions.SpectrumQualityX = value; FireProjectChanged(); } }
        public int SpectrogramQualityY      { get => ViewOptions.SpectrumQualityY; set { ViewOptions.SpectrumQualityY = value; FireProjectChanged(); } }

        public int AttackC { get => Project?.AttackC ?? 0; set { Project.AttackC = value; FireProjectChanged(); RedrawPoints(); } }
        public int AttackV { get => Project?.AttackV ?? 0; set { Project.AttackV = value; FireProjectChanged(); RedrawPoints(); } }
        public int AttackR { get => Project?.AttackR ?? 0; set { Project.AttackR = value; FireProjectChanged(); RedrawPoints(); } }
        public int DecayC  { get => Project?.DecayC ?? 0;  set { Project.DecayC = value; FireProjectChanged(); RedrawPoints(); } }
        public int DecayV  { get => Project?.DecayV ?? 0;  set { Project.DecayV = value; FireProjectChanged(); RedrawPoints(); } }
        public int DecayR  { get => Project?.DecayR ?? 0;  set { Project.DecayR = value; FireProjectChanged(); RedrawPoints(); } }

        public ViewOptions ViewOptions { get; set; } = new ViewOptions();
        public bool DoShowWaveform => ViewOptions != null && ViewOptions.DoShowWaveform;
        public bool DoShowPitch => ViewOptions != null && ViewOptions.DoShowPitch;
        public bool DoShowSpectrum => ViewOptions != null && ViewOptions.DoShowSpectrum;

        public bool MustHideNotEnabled
        {
            get => Project != null && Project.ProjectOptions.MustHideNotEnabled;
            set { Project.ProjectOptions.MustHideNotEnabled = value; UpdatePagerCollection(); }
        }
        public bool MustHideCompleted
        {
            get => Project != null && Project.ProjectOptions.MustHideCompleted;
            set { Project.ProjectOptions.MustHideCompleted = value; UpdatePagerCollection(); }
        }

        public string Prefix { get => Project?.Prefix; set { Project.Prefix = value; FireProjectChanged(); } }
        public string Suffix { get => Project?.Suffix; set { Project.Suffix = value; FireProjectChanged(); } }

        public string WavPrefix
        {
            get => Project?.WavPrefix;
            set
            {
                Project.WavPrefix = value;
                SaveSystem.SaveImmediately();
                ReloadProjectCommand.Execute(0);
            }
        }
        public string WavSuffix
        {
            get => Project?.WavSuffix;
            set
            {
                Project.WavSuffix = value;
                SaveSystem.SaveImmediately();
                ReloadProjectCommand.Execute(0);
            }
        }

        public int UserScaleYView
        {
            get => (int)(Settings.UserScaleY * 100);
            set
            {
                if (value <= 0)
                    return;
                Project.UserScaleY = value / 100.0;
                SaveSystem.SaveImmediately();
                ReloadProjectCommand.Execute(0);
            }
        }
        public int UserScaleXView
        {
            get => (int)(Settings.UserScaleX * 100);
            set
            {
                if (value <= 0)
                    return;
                Project.UserScaleX = value / 100.0;
                SaveSystem.SaveImmediately();
                ReloadProjectCommand.Execute(0);
            }
        }

        public PhonemeType Mode { get => Settings.Mode; set => Settings.Mode = value; }
        public string ModeSymbol => SymbolOfType(Mode);

        public PhonemeType ModeC => PhonemeType.Consonant;
        public PhonemeType ModeV => PhonemeType.Vowel;
        public PhonemeType ModeR => PhonemeType.Rest;

        public string ModeCSymbol => SymbolOfType(ModeC);
        public string ModeVSymbol => SymbolOfType(ModeV);
        public string ModeRSymbol => SymbolOfType(ModeR);

        public bool IsToolsPanelShown { get => isToolsPanelShown; set { isToolsPanelShown = value; RaisePropertyChanged(() => IsToolsPanelShown); } }

        public bool IsLoading { get; set; }
        public bool IsNotLoading => !IsLoading;
        public bool IsOtoPreviewMode { get; set; }

        public static bool IsDebug { get; set; }

        public string Title => GetTitle();

        public MainWindowViewModel()
        {
#if DEBUG
            IsDebug = true;
#endif
            SaveSystem = new SaveSystem(ProjectManager);
            SaveSystem.OnSaved += delegate { RaisePropertyChanged(nameof(Title)); };
        }

        #region private

        private bool isToolsPanelShown = true;

        private async void LoadProjectAsync()
        {
            SaveSystem.Stop();
            IsLoading = true;
            RaisePropertyChanged(() => IsLoading);
            if (Project != null && Project.IsLoaded)
            {
                ViewOptions = Project.ViewOptions;
                ImagesLibrary.ViewOptions = ViewOptions;
                var containers = new List<PagerContentBase>();
                for (var i = 0; i < Project.ProjectLines.Count; i++)
                {
                    var index = i;
                    await Task.Run(() => ExceptionCatcher.Current.CatchOnAction(() =>
                    {
                        if (Project.ProjectLines[index].Recline != null)
                        {
                            containers.Add(CreateProjectLineContainer(index));
                        };
                    }));
                }

                WavControlsPagerViewModel = new PagerViewModel(containers, ViewOptions, ImagesLibrary, false);
                PagerViewModel = WavControlsPagerViewModel;
                PagerViewModel.PagerChanged += delegate { RaisePropertyChanged(() => Title); };
                Project.OnBeforeSave += WriteProjectOptions;
                PagerViewModel.ReadProjectOption(Project.ProjectOptions);
                UpdatePagerCollection();
                OtoGenerator = new OtoGenerator(Project);
                Project.OnProjectChanged += delegate { RaisePropertyChanged(nameof(Title)); };
            }
            IsLoading = false;
            App.MainDispatcher.Invoke(Refresh);
            SaveSystem.SaveImmediately();
            SaveSystem.Start();
            RaisePropertiesChanged(nameof(OpenVoicebankFolderCommand), nameof(OpenProjectFolderCommand));
        }

        private void HandleGoto(PagerContentBase content)
        {
            if (IsOtoPreviewMode && content is ProjectLineContainer projectLineContainer)
            {
                SetOtoMode(projectLineContainer);
            }
            else
            {
                PagerViewModel.Goto(content);
            }
        }

        private void WriteProjectOptions()
        {
            PagerViewModel.WriteProjectOptions(Project.ProjectOptions);
        }

        private ProjectLineContainer CreateProjectLineContainer(int i)
        {
            var projectLine = Project.ProjectLines[i];
            var sampleName = Project.Voicebank.GetSamplePath(projectLine.Recline.Name, Project.WavPrefix, Project.WavSuffix);
            var hash = new WavImagesHash()
            {
                Reclist = Project.Reclist.Name,
                Voicebank = Project.Voicebank.Name,
                UserScaleX = Settings.UserScaleX,
                UserScaleY = Settings.UserScaleY,
                Recline = projectLine.Recline.Name,
                Suffix = Project.WavSuffix,
                Prefix = Project.WavPrefix
            };

            var container = new ProjectLineContainer(projectLine, ImagesLibrary, WavPlayer, sampleName, hash, i);
            container.OnOtoRequested += model => SetOtoMode(container);
            container.OnAddPointRequested += (position, type) => HandleAddPointRequest(container, position, type);
            container.OnGenerateOtoRequested += delegate
            {
                if (!IsOtoPreviewMode)
                    return;
                Project.ResetOto();
                projectLine.Recline.ResetOto();
                OtoGenerator.GenerateFromProjectLine(projectLine);
                OtoPagerViewModel.UpdateOtoPreviewControls(container.GenerateOtoPreview());
                RaisePropertyChanged(() => PagerViewModel);
            };
            container.OnChangePhonemeModeRequested += delegate (PhonemeType type)
            {
                SetPhonemeModeCommand.Execute(type);
            };
            return container;
        }

        private void HandleAddPointRequest(ProjectLineContainer container, double position, PhonemeType type)
        {
            if (Project.Reclist.WavMask.CanAddPoint(type, container.ProjectLine))
                container.AddPoint(position, type);
        }

        private void Refresh()
        {
            if (Project == null)
                return;
            RaisePropertiesChanged(
                () => VoicebankName,
                () => VoicebankSubfolder,
                () => VoicebankImagePath,
                () => PagerViewModel
            );

            RaisePropertiesChanged(
                () => AttackC,
                () => AttackV,
                () => AttackR
            );

            RaisePropertiesChanged(
                () => DecayC,
                () => DecayV,
                () => DecayR
            );

            RaisePropertiesChanged(
                () => WavPrefix,
                () => WavSuffix,
                () => Prefix,
                () => Suffix
            );

            RaisePropertiesChanged(
                () => UserScaleYView,
                () => UserScaleXView,
                () => Title,
                () => ReclistName,
                () => VoicebankImage
            );

            RaisePropertiesChanged(
                () => MustHideCompleted,
                () => MustHideNotEnabled,
                () => IsProjectReady
            );
            RaisePropertyChanged(() => IsLoading);
        }


        private void ResetProject()
        {
            PagerViewModel?.Clear();
            ProjectManager.Reset();
            IsLoading = true;
            Refresh();
        }

        private string GetTitle()
        {
            var projectFileName = Project != null && Project.IsLoaded ? " " + Path.GetFileName(Settings.ProjectFile) : "";
            if (Project == null)
                return $"WavConfig {Version} {projectFileName}";
            if (PagerViewModel == null || PagerViewModel.PagesTotal == 0)
                return $"WavConfig {Version} {projectFileName}  [{Project.Voicebank.Name}] : {Project.Reclist.Name}";
            return $"WavConfig {Version} {projectFileName}{(Project.IsChangedAfterBackup ? "*" : "")} [{Project.Voicebank.Name}] : {Project.Reclist.Name} | " +
                $"Page {PagerViewModel.CurrentPage + 1}/{PagerViewModel.PagesTotal}";
        }

        private void SetOtoMode(ProjectLineContainer container)
        {
            if (IsOtoPreviewMode)
            {
                SetWavConfigMode.Execute(null);
            }
            else
            {
                Project.ResetOto();
                container.ProjectLine.Recline.ResetOto();
                OtoGenerator.GenerateFromProjectLine(container.ProjectLine);
                OtoPagerViewModel = new PagerViewModel(container.GenerateOtoPreview(), ViewOptions, ImagesLibrary, true);
                OtoPagerViewModel.SetBase(container);
                OtoPagerViewModel.OtoMode();
                OtoPagerViewModel.SetPageSizeCommand.Execute(Project.ProjectOptions.OtoPageSize);
                PagerViewModel = OtoPagerViewModel;
                IsOtoPreviewMode = true;
                Refresh();
            }
        }

        private BitmapImage GetVoicebankImage()
        {
            if (Project?.Voicebank != null && Project.Voicebank.ImagePath != "" && File.Exists(Project.Voicebank.ImagePath))
                return new BitmapImage(new Uri(Project.Voicebank.ImagePath));
            else
                return null;
        }

        private string SymbolOfType(PhonemeType type)
        {
            return type.ToString().Substring(0, 1);
        }

        private void FireProjectChanged()
        {
            if (Project != null && Project.IsLoaded)
                Project.FireChanged();
        }

        private void RedrawPoints()
        {
            var controls = PagerViewModel.PageContent;
            foreach (var control in controls)
            {
                control.HandlePointsChanged();
            }
            if (IsOtoPreviewMode)
            {
                PagerViewModel.Base.RequestGenerateOto();
            }
        }

        private void UpdatePagerCollection()
        {
            GotoUserControlViewModel = null;
            PagerViewModel.RequestUpdateCollection();
            GotoUserControlViewModel = new GotoUserControlViewModel();
            GotoUserControlViewModel.SetItems(PagerViewModel.Collection);
            GotoUserControlViewModel.OnGoto += HandleGoto;
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
                SaveSystem.SaveImmediately();
                ResetProject();
                Settings.ProjectFile = filename;
                ProjectManager.CreateProject(filename);
                CallProjectCommand.Execute(this);
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
                Settings.ProjectFile = (string)obj;
                SaveSystem.SaveImmediately();
                Refresh();
                RaisePropertyChanged(nameof(Title));
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
            ProjectViewModel.ProjectDataChanged += LoadProjectAsync;
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
                SaveSystem.SaveImmediately();
            }
        },
        "Save Oto",
        "Oto Files|*oto*.ini|*|*",
        delegate
        {
            return Project != null && Project.IsLoaded;
        },
        "oto");

        public ICommand SaveCommand => new DelegateCommand(delegate
        {
            SaveSystem.SaveImmediately();
        }, () => Project != null && Project.IsLoaded);

        public ICommand GenerateOremoPackCommand => new DelegateCommand(delegate
        {
            var oremoPack = OremoPackGenerator.Generate(Project.Reclist);
            OremoPackWriter.Current.WriteAndOpenFolder(oremoPack);

        },() => Project != null && Project.Reclist != null && Project.Reclist.IsLoaded);

        public ICommand LoadedCommand => new DelegateCommand(() =>
        {
            ExceptionCatcher.Current.CatchOnAction(() => ProjectManager.LoadProject(Settings.ProjectFile), "Failed to load project.");
            LoadProjectAsync();
            Refresh();
        }, () => true);

        public ICommand ReloadProjectCommand => new DelegateCommand(() =>
        {
            var hadProject = Project != null && Project.IsLoaded;
            SetWavConfigMode.Execute(null);
            ExceptionCatcher.Current.CatchOnAction(() => ProjectManager.LoadProject(Settings.ProjectFile), "Failed to load project.");
            LoadProjectAsync();
            Refresh();
            if (hadProject)
                UpdatePagerCollection();
        }, () => !IsLoading);

        public ICommand SetWavConfigMode => new DelegateCommand(() =>
        {
            OtoPagerViewModel.UnsubscribeBaseChanged();
            PagerViewModel = WavControlsPagerViewModel;
            IsOtoPreviewMode = false;
            Refresh();
        }, () => IsOtoPreviewMode);

        public ICommand TogglePitchCommand => new DelegateCommand(() =>
        {
            ViewOptions.DoShowPitch = !ViewOptions.DoShowPitch;
            RaisePropertyChanged(nameof(DoShowPitch));
            PagerViewModel.RefreshPageContent();
        }, () => Project != null && Project.IsLoaded);

        public ICommand ToggleWaveformCommand => new DelegateCommand(() =>
        {
            ViewOptions.DoShowWaveform = !ViewOptions.DoShowWaveform;
            RaisePropertyChanged(nameof(DoShowWaveform));
            PagerViewModel.RefreshPageContent();
        }, () => Project != null && Project.IsLoaded);

        public ICommand ToggleSpectrumCommand => new DelegateCommand(() =>
        {
            ViewOptions.DoShowSpectrum = !ViewOptions.DoShowSpectrum;
            RaisePropertyChanged(nameof(DoShowSpectrum));
            PagerViewModel.RefreshPageContent();
        }, () => Project != null && Project.IsLoaded);

        public ICommand OpenSettingsFolderCommand => new OpenFolderCommand(PathResolver.Current.Reclist());
        public ICommand OpenBackupsFolderCommand => new OpenFolderCommand(PathResolver.Current.Backup(true));
        public ICommand OpenVoicebankFolderCommand => new OpenFolderCommand(PathResolver.Current.TryGetDirectoryName(Project?.Voicebank?.Fullpath), Project?.Voicebank != null);
        public ICommand OpenProjectFolderCommand => new OpenFolderCommand(PathResolver.Current.TryGetDirectoryName(Settings.ProjectFile), Project != null && Project.IsLoaded);

#if DEBUG

        public ICommand DebugCommand => new DelegateCommand(() =>
        {

        }, () => IsDebug);

        public ICommand CvcFromNoskipCommand => new DelegateCommand(() =>
        {
            foreach (var projectLine in Project.ProjectLines)
            {
                if (projectLine.Recline == null)
                    continue;
                if (Project.Reclist.WavMask.IsInGroup(projectLine.Recline.Name, "CV-VC") && !Project.Reclist.WavMask.IsInGroup(projectLine.Recline.Name, "C"))
                {
                    if (projectLine.ConsonantPoints.Count == 6)
                    {
                        var points = new List<int>()
                        {
                            projectLine.ConsonantPoints[2],
                            projectLine.ConsonantPoints[3]
                        };
                        projectLine.ConsonantPoints = points;
                    }
                }
                else if (Project.Reclist.WavMask.IsInGroup(projectLine.Recline.Name, "VC special"))
                {
                    if (projectLine.RestPoints.Count == 2)
                    {
                        var points = new List<int>()
                        {
                            projectLine.RestPoints[1]
                        };
                        projectLine.RestPoints = points;
                    }
                }
            }
            Project.FireChanged();
            ReloadProjectCommand.Execute(0);
        }, () => Project != null && Project.IsLoaded && IsDebug);


#endif
        #endregion
    }
}
