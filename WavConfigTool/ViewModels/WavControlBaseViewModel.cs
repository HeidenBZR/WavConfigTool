using DevExpress.Mvvm;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using WavConfigCore;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public abstract class WavControlBaseViewModel : ViewModelBase
    {
        public static int GlobalHeight => 100;
        public int Height => GlobalHeight;
        public int Space => 30;

        public virtual bool IsCompleted => false;
        public virtual bool IsEnabled => true;
        public virtual string ViewName => "{NAME}";

        public bool IsReadyToDrawPoints { get; private set; }

        // TODO: Move base things here

        public virtual void Ready() { }

        public event SimpleHandler PointsChanged = delegate { };
        public ImagesLibrary ImagesLibrary;

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

        public virtual void HandlePointsChanged()
        {

        }

        public virtual void HandleViewChanged()
        {

        }

        public virtual void SetReady(bool ready)
        {
            IsReadyToDrawPoints = ready;
        }

    }
}
