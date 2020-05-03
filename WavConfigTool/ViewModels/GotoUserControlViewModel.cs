using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    class GotoUserControlViewModel : ViewModelBase
    {
        public delegate void GotoHandler(WavControlBaseViewModel viewModel);
        public event GotoHandler OnGoto;

        public ObservableCollection<WavControlBaseViewModel> Items { get; set; }
        public WavControlBaseViewModel SelectedItem { get; set; }

        public void SetItems(ObservableCollection<WavControlBaseViewModel> items)
        {
            Items = items;
            SelectedItem = null;
        }

        #region private

        private void Goto(WavControlBaseViewModel model)
        {
            OnGoto(model);
        }

        #endregion

        #region commands

        public ICommand GotoCommand => new DelegateCommand(delegate
        {
            Goto(SelectedItem);
        }, () => Items != null && SelectedItem != null);

        #endregion

    }
}
