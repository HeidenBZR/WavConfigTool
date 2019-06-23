using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
    /// Логика взаимодействия для OtoPreviewControl.xaml
    /// </summary>
    public partial class OtoPreviewControl : WavControlBase
    {
        public double Left;

        public OtoPreviewControl()
        {
            InitializeComponent();
        }
    }
}
