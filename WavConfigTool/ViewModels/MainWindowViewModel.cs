﻿using DevExpress.Mvvm;
using System;
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
                        () => Page,
                        () => PageTotal,
                        () => ItemsOnPage);

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
                        () => VoicebankImage
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

        public int Page { get => _page + 1; set => _page = value - 1; }
        public int PageTotal { get => _pageTotal + 1; set => _pageTotal = value - 1; }
        public int ItemsOnPage { get => _itemsOnPage + 1; set => _itemsOnPage = value - 1; }

        public int ConsonantAttack { get => Project.ConsonantAttack; set => Project.ConsonantAttack = value; }
        public int VowelAttack { get => Project.VowelAttack; set => Project.VowelAttack = value; }
        public int VowelSustain { get => Project.VowelSustain; set => Project.VowelSustain = value; }

        public string Prefix { get => Project.Prefix; set => Project.Prefix = value; }
        public string Suffix { get => Project.Suffix; set => Project.Suffix = value; }

        public double WavAmplitudeMultiplayer { get; set; }

        public WavConfigPoint Mode { get; set; } = WavConfigPoint.V;

        public WavConfigPoint ModeC { get; } = WavConfigPoint.C;
        public WavConfigPoint ModeV { get; } = WavConfigPoint.V;
        public WavConfigPoint ModeR { get; } = WavConfigPoint.R;

        public string ProjectSavedString { get => Settings.IsUnsaved ? "*" : ""; }
        public string Title
        {
            get => $"WavConfig v.{Version.ToString()} - {ProjectSavedString}  [{Project.Voicebank.Name}] : {Project.Reclist.Name}";
        }

        public ObservableCollection<WavControlViewModel> WavControlViewModels { get; set; } = new ObservableCollection<WavControlViewModel>();


        //Point PrevMousePosition;
        private int _page;
        private int _pageTotal;
        private int _itemsOnPage;
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
            await Task.Run(() => LoadProject());
            WavControlViewModels = new ObservableCollection<WavControlViewModel>();
            foreach (var line in Project.ProjectLines)
                WavControlViewModels.Add(new WavControlViewModel(line));
            RaisePropertiesChanged("WavControlViewModels");
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
            if (!Project.IsLoaded)
            {
                CallProjectCommand.Execute(Project);
            }
        }

        public ICommand SetMode
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    Mode = (WavConfigPoint)obj;
                }, param => (param != null));
            }
        }

        public ICommand CallProjectCommand
        {
            get
            {
                return new DelegateCommand((obj) =>
                {
                    ViewManager.CallProject(new ProjectViewModel(Project));
                }, (param) => (Project != null));
            }
        }


        //void ClearTemp()
        //{
        //    try
        //    {
        //        foreach (string filename in System.IO.Directory.GetFiles(Settings.TempDir))
        //            System.IO.File.Delete(filename);
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on clear cache",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //async void ClearTempAsync()
        //{
        //    try
        //    {
        //        foreach (string filename in System.IO.Directory.GetFiles(Settings.TempDir))
        //            await Task.Run(delegate { System.IO.File.Delete(filename); });
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show($"{ex.Message}\r\n{ex.StackTrace}", "Error on clear cache async",
        //            MessageBoxButton.OK, MessageBoxImage.Error);
        //    }
        //}

        //void Init()
        //{
        //    PrevMousePosition = Mouse.GetPosition(this); ??????

        //}

    }
}
