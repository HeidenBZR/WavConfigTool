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

namespace WavConfigTool.ViewTools
{

    public class DragDeltaToPointArgsConverter : EventArgsConverterBase<DragDeltaEventArgs>
    {
        protected override object Convert(object sender, DragDeltaEventArgs args)
        {
            return new Point(
                args.HorizontalChange / Settings.ScaleX,
                args.VerticalChange / Settings.ScaleY
            );
        }
    }
}
