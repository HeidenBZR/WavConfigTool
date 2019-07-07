using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WavConfigTool.ViewModels
{
    public abstract class WavControlBaseViewModel : ViewModelBase
    {
        public abstract void Load();

        public static void SortPoints(ObservableCollection<WavPointViewModel> collection)
        {
            var sortableList = new List<WavPointViewModel>(collection);
            sortableList.Sort((n1, n2) => n1.Position.CompareTo(n2.Position));

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }
    }
}
