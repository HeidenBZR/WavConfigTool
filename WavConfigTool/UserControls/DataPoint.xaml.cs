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

namespace WavConfigTool.UserControls
{
    /// <summary>
    /// Логика взаимодействия для DataPoint.xaml
    /// </summary>
    public partial class DataPoint : UserControl
    {
        /// <summary>
        /// Точка. Биндит свой дата-контекст как положение слева.
        /// </summary>
        public DataPoint()
        {
            InitializeComponent();
        }
    }
}
