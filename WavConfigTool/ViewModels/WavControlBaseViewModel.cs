using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace WavConfigTool.ViewModels
{
    public abstract class WavControlBaseViewModel : ViewModelBase
    {
        public abstract void Ready();

        public event SimpleHandler PointsChanged;

        public static void SortPoints(ObservableCollection<WavPointViewModel> collection)
        {
            var sortableList = new List<WavPointViewModel>(collection);
            sortableList.Sort((n1, n2) => n1.Position.CompareTo(n2.Position));

            for (int i = 0; i < sortableList.Count; i++)
            {
                collection.Move(collection.IndexOf(sortableList[i]), i);
            }
        }

        public void FirePointsChanged()
        {
            PointsChanged();
        }
    }
}
