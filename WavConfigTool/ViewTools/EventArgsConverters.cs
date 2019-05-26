using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DevExpress.Mvvm.UI;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using WavConfigTool.UserControls;
using WavConfigTool.ViewModels;
using WavConfigTool.Tools;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewTools
{

    public class DragDeltaToPointArgsConverter : EventArgsConverterBase<DragDeltaEventArgs>
    {
        protected override object Convert(object sender, DragDeltaEventArgs args)
        {
            return new Point(
                args.HorizontalChange,
                args.VerticalChange
            );
        }
    }

    public class MouseButtonEventArgsConverter : EventArgsConverterBase<MouseButtonEventArgs>
    {
        protected override object Convert(object sender, MouseButtonEventArgs args)
        {
            var ob = (UIElement)sender;
            return new Point(
                Mouse.GetPosition(ob).X,
                Mouse.GetPosition(ob).Y
            );
        }
    }
}
