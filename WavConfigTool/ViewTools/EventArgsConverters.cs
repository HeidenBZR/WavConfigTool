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
using System.Windows.Media;
using System.Windows.Controls;

namespace WavConfigTool.ViewTools
{

    public class DragDeltaToPointConverter : EventArgsConverterBase<DragDeltaEventArgs>
    {
        protected override object Convert(object sender, DragDeltaEventArgs args)
        {
            return new Point(
                Canvas.GetLeft(sender as UIElement) + args.HorizontalChange,
                Canvas.GetLeft(sender as UIElement) + args.VerticalChange
            );
        }
    }

    public class MouseMoveToPointConverter : EventArgsConverterBase<MouseEventArgs>
    {
        protected override object Convert(object sender, MouseEventArgs args)
        {
            var ob = (UIElement)sender;
            var parent = sender;
            UIElement wavControl = null;
            while (wavControl == null)
            {
                wavControl = VisualTreeHelper.GetParent(ob) as WavControl;
                parent = VisualTreeHelper.GetParent(ob);
            }
            return new Point(
                Mouse.GetPosition(wavControl).X,
                Mouse.GetPosition(wavControl).Y
            );
        }
    }

    public class MouseButtonToPointConverter : EventArgsConverterBase<MouseButtonEventArgs>
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
