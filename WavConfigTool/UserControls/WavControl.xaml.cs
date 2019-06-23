using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WavConfigTool.Classes;
using WavConfigTool.Tools;
using WavConfigTool.UserControls;
using WavConfigTool.Windows;

namespace WavConfigTool.UserControls
{
    /// <summary>
    /// Логика взаимодействия для WavControl.xaml
    /// </summary>
    public partial class WavControl : WavControlBase
    {

        public WavControl()
        {
            InitializeComponent();
        }


        #region Events

        //private void WavControl_Reset(object sender, RoutedEventArgs e)
        //{
        //    string tag = (sender as MenuItem).Tag.ToString();
        //    if (tag == "All") Reset();
        //    else if (tag == "C") Reset(WavConfigPoint.C);
        //    else if (tag == "V") Reset(WavConfigPoint.V);
        //    else if (tag == "D") Reset(WavConfigPoint.D);
        //}

        //private void WavControl_OtoPreview(object sender, RoutedEventArgs e)
        //{
        //    DrawOtoPreview();
        //}

        //private void Main_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        //{
        //    if (Keyboard.IsKeyDown(Key.LeftAlt) || Keyboard.IsKeyDown(Key.RightAlt))
        //    {
        //        if (AllowOtoPreview)
        //            DrawOtoPreview();
        //    }
        //    else
        //    {
        //        if (!WavContextMenu.IsVisible && Keyboard.IsKeyUp(Key.Space))
        //        {
        //            double x = e.GetPosition(this).X;
        //            Draw(MainWindow.Mode, x);
        //        }
        //    }
        //}

        #endregion
    }
}
