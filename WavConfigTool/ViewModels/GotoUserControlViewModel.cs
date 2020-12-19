using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    class GotoUserControlViewModel : ViewModelBase
    {
        public delegate void GotoHandler(PagerContentBase viewModel);
        public event GotoHandler OnGoto;

        public string SelectedKey { get; set; }
        public string[] ItemsKeys { get; set; }
        public bool IsCanGoto => ItemsKeys != null && SelectedKey != null && itemsByName.ContainsKey(SelectedKey);

        public void SetItems(IList<PagerContentBase> items)
        {
            SelectedKey = null;
            itemsByName.Clear();
            ItemsKeys = new string[items.Count()];
            var i = 0;
            foreach (var item in items)
            {
                var viewName = item.GetViewName();
                itemsByName[viewName] = item;
                ItemsKeys[i] = viewName;
                i++;
            }
        }

        #region private

        private Dictionary<string, PagerContentBase> itemsByName = new Dictionary<string, PagerContentBase>();

        private void Goto(PagerContentBase model)
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
