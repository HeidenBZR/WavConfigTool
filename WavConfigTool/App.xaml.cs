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
using System.Windows.Threading;

namespace WavConfigTool
{
    public delegate void SimpleHandler();
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static Dispatcher MainDispatcher;

        public App()
        {
            MainDispatcher = Dispatcher.CurrentDispatcher;
        }
    }
}
