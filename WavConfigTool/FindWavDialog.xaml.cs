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
using System.Windows.Shapes;

namespace WavConfigTool
{
    /// <summary>
    /// Логика взаимодействия для FindWav.xaml
    /// </summary>
    public partial class FindWavDialog : Window
    {
        public int Index = -1;

        public FindWavDialog(Reclist reclist)
        {
            InitializeComponent();
            ComboboxReclines.Items.Clear();
            ComboboxReclines.ItemsSource = reclist.Reclines;
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            Index = -1;
            Close();
        }

        private void ButtonGoto_Click(object sender, RoutedEventArgs e)
        {
            Index = ComboboxReclines.SelectedIndex;
            Close();
        }
    }
}
