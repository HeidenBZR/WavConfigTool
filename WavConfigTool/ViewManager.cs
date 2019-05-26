using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WavConfigTool.Windows;
using WavConfigTool.UserControls;
using WavConfigTool.ViewModels;

namespace WavConfigTool
{
    public class ViewManager
    {
        private static ViewManager _instance;
        public static ViewManager Instance { get { if (_instance is null) _instance = new ViewManager(); return _instance; } }

        public ProjectWindow ProjectWindow { get; set; }
        public static bool CallProject(ProjectViewModel projectViewModel)
        {
            Instance.ProjectWindow = new ProjectWindow() { DataContext = projectViewModel };
            var result = Instance.ProjectWindow.ShowDialog();
            return result.HasValue && result.Value;
        }
    }
}
