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

        public string SelectedKey { get; set; }
        public string[] ItemsKeys { get; set; }
        public bool IsCanGoto => ItemsKeys != null && SelectedKey != null && itemsByName.ContainsKey(SelectedKey);

        public void SetItems(ObservableCollection<WavControlBaseViewModel> items)
        {
            this.items = items;
            SelectedKey = null;
            itemsByName.Clear();
            ItemsKeys = new string[items.Count()];
            var i = 0;
            foreach (var item in items)
            {
                itemsByName[item.ViewName] = item;
                ItemsKeys[i] = item.ViewName;
                i++;
            }
        }

        #region private

        private Dictionary<string, WavControlBaseViewModel> itemsByName = new Dictionary<string, WavControlBaseViewModel>();
        private ObservableCollection<WavControlBaseViewModel> items { get; set; }

        private void Goto(WavControlBaseViewModel model)
        {
            OnGoto(model);
        }

        #endregion

        #region commands

        public ICommand GotoCommand => new DelegateCommand(delegate
        {
            Goto(itemsByName[SelectedKey]);
        }, () => IsCanGoto);

        #endregion

    }
}
