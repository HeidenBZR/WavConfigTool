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
        public int Height { get; set; }
        public int Space => 30;

        public virtual bool IsCompleted => false;
        public virtual bool IsEnabled => true;
        public virtual string ViewName { get; } = "{NAME}";

        public bool IsReadyToDrawPoints { get; private set; }

        public PagerContentBase PagerContent { get; protected set; }
        public WaveForm WaveForm { get; protected set; }

        public ViewOptions ViewOptions { get; set; }

        // TODO: Move base things here

        public virtual void Ready() { }
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

        public abstract void Update(PagerContentBase pagerContent);

    }
}
