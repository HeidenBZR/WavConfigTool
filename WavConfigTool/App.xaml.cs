using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using WavConfigTool.Classes;
using WavConfigTool.Windows;
using WavConfigTool.ViewModels;

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        //public DisplayRootRegistry displayRootRegistry = new DisplayRootRegistry();
        //MainWindowViewModel mainWindowViewModel;

        //public App()
        //{
        //    displayRootRegistry.RegisterWindowType<MainWindowViewModel, MainWindow>();
        //    //displayRootRegistry.RegisterWindowType<ProjectViewModel, ProjectWindow>();
        //}

        //protected override async void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    mainWindowViewModel = new MainWindowViewModel();

        //    await displayRootRegistry.ShowModalPresentation(mainWindowViewModel);

        //    Shutdown();
        //}

    }
}
