using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class FrqPointViewModel : ViewModelBase
    {
        public double X { get; set; }
        public double Y { get; set; }

        public FrqPointViewModel() { }

        public FrqPointViewModel(double x, double y)
        {
            this.X = x;
            this.Y = y;
        }
    }
}
