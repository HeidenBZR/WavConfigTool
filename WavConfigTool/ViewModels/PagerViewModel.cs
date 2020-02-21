using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    public class PagerViewModel : ViewModelBase
    {
        public WavControlBaseViewModel Base { get; set; }

        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }
        public int ItemsCount { get; set; } = 0;
        public int PagesTotal => GetPagesTotal();
        public int CurrentPage
        {
            get => _currentPage;
            set => SetPageCommand.Execute(value);
        }
        public int CurrentPageView { get => CurrentPage + 1; set => CurrentPage = value - 1; }
        public bool IsHidden { get; set; } = false;
        public bool IsOtoMode { get; set; } = false;

        public ObservableCollection<WavControlBaseViewModel> Collection { get; private set; } = new ObservableCollection<WavControlBaseViewModel>();
        public ObservableCollection<WavControlBaseViewModel> PageContent => GetPageContent();

        public event SimpleHandler PagerChanged = delegate { };

        public PagerViewModel()
        {
            RaisePropertyChanged(() => PageContent);
            IsHidden = false;
        }

        public PagerViewModel(ObservableCollection<WavControlBaseViewModel> collection)
        {
            Collection = collection;
            RaisePropertyChanged(() => PageContent);
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PagesTotal);
            IsHidden = false;
            ItemsCount = Collection.Count();
        }

        public void Clear()
        {
            _currentPage = 0;
            Collection.Clear();
            IsHidden = true;
        }

        public void ReadProjectOption(ProjectOptions projectOptions)
        {
            SetPageSizeCommand.Execute(projectOptions.PageSize);
            SetPageCommand.Execute(projectOptions.LastPage);
        }

        public void WriteProjectOptions(ProjectOptions projectOptions)
        {
            projectOptions.LastPage = CurrentPage;
            projectOptions.PageSize = PageSize;
        }

        public void OtoMode()
        {
            CurrentPage = 0;
            IsOtoMode = true;
            Refresh();
        }

        public void UpdateOtoPreviewControls(ObservableCollection<WavControlBaseViewModel> controls)
        {
            while (Collection.Count > 0)
            {
                Collection.RemoveAt(Collection.Count - 1);
            }
            foreach (var control in controls)
            {
                Collection.Add(control);
            }
            IsHidden = false;
            ItemsCount = Collection.Count();
        }

        public void WaitForPageLoadedAndLoadRest()
        {
            var pageContentLocal = pageContent.ToArray();
            foreach (var control in pageContentLocal)
            {
                var wavControl = control as WavControlViewModel;
                if (wavControl == null)
                    return;
                wavControl.OnLoaded += () =>
                {
                    foreach (var innerControl in pageContentLocal)
                    {
                        var innerWavControl = control as WavControlViewModel;
                        if (innerWavControl == null || !innerWavControl.IsLoaded)
                            return;
                        LoadRest();
                    }
                };
            }
        }

        #region private

        private ObservableCollection<WavControlBaseViewModel> pageContent = new ObservableCollection<WavControlBaseViewModel>();
        private int _currentPage = 0;
        private int _pageSize = 7;

        private ObservableCollection<WavControlBaseViewModel> GetPageContent()
        {
            if (IsHidden)
                return new ObservableCollection<WavControlBaseViewModel>();
            if (IsOtoMode)
            {
                if (pageContent.Count == 0)
                {
                    pageContent.Add(Base);
                }
                else if (pageContent[0] != Base)
                {
                    pageContent[0] = Base;
                }
                while (pageContent.Count > 1)
                {
                    pageContent.RemoveAt(1);
                }
                var otosPageSize = PageSize - 1;
                for (int i = otosPageSize * CurrentPage; i < otosPageSize * (CurrentPage + 1) && i < Collection.Count; i++)
                {
                    pageContent.Add(Collection[i]);
                }
            }
            else
            {
                pageContent.Clear();
                for (int i = PageSize * CurrentPage; i < PageSize * (CurrentPage + 1); i++)
                {
                    if (i < Collection.Count)
                        pageContent.Add(Collection[i]);
                }
            }
            return pageContent;
        }

        private void Refresh()
        {
            RaisePropertyChanged(() => PageSize);
            RaisePropertyChanged(() => PagesTotal);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }

        private int GetPagesTotal()
        {
            if (ItemsCount == 0)
                return 1;
            var realSize = IsOtoMode ? PageSize - 1 : PageSize;
            var pages = ItemsCount / realSize;
            if (ItemsCount % realSize > 0)
                pages++;
            return pages;
        }

        private void LoadRest()
        {
            foreach (var control in Collection)
            {
                var wavControl = control as WavControlViewModel;
                if (wavControl == null || wavControl.IsLoaded || wavControl.IsLoading)
                    continue;
                wavControl.LoadExternal();
            }

        }

        #endregion

        #region commands

        public ICommand SetFirstPageCommand => new DelegateCommand(delegate
        {
            CurrentPage = 0;
        }, () => PagesTotal > 0);

        public ICommand SetLastPageCommand => new DelegateCommand(delegate
        {
            if (PagesTotal > 0)
                CurrentPage = PagesTotal - 1;
            else
                CurrentPage = 0;
        }, () => PagesTotal > 0);

        public ICommand SetNextPageCommand => new DelegateCommand(delegate
        {
            CurrentPage++;
        }, () => PagesTotal > 0 && CurrentPage < PagesTotal - 1);

        public ICommand SetPreiousPageCommand => new DelegateCommand(delegate
        {
            CurrentPage--;
        }, () => PagesTotal > 0 && CurrentPage > 0);

        public ICommand SetPageCommand => new DelegateCommand<int>((currentPage) =>
        {
            _currentPage = currentPage;
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }, currentPage => (currentPage < PagesTotal && currentPage >= 0));

        public ICommand SetPageSizeCommand => new DelegateCommand<int>((pageSize) =>
        {
            var current = _pageSize * _currentPage;
            _pageSize = pageSize;
            Refresh();
            SetPageCommand.Execute(current / pageSize);
        }, pageSize => pageSize > 0);

        #endregion
    }
}
