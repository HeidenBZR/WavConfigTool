using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WavConfigCore;

namespace WavConfigTool.ViewModels
{
    public class PagerViewModel : ViewModelBase
    {
        public WavControlViewModel Base { get; set; }

        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }
        public int ItemsCount => Collection != null ? Collection.Count : 0;
        public int PagesTotal => GetPagesTotal();
        public int CurrentPage
        {
            get => _currentPage;
            set => SetPageCommand.Execute(value);
        }
        public int CurrentPageView { get => CurrentPage + 1; set => CurrentPage = value - 1; }
        public bool IsHidden { get; set; } = false;
        public bool IsOtoMode { get; set; } = false;

        public List<WavControlBaseViewModel> SourceCollection { get; private set; } = new List<WavControlBaseViewModel>();
        public ObservableCollection<WavControlBaseViewModel> Collection { get; private set; } = new ObservableCollection<WavControlBaseViewModel>();
        public ObservableCollection<WavControlBaseViewModel> PageContent => pageContent;

        public event SimpleHandler PagerChanged = delegate { };

        public PagerViewModel()
        {
            RaisePropertyChanged(() => PageContent);
            IsHidden = false;
        }

        public PagerViewModel(List<WavControlBaseViewModel> collection)
        {
            SourceCollection = collection;
            UpdateCollection();
            RaisePropertyChanged(() => PageContent);
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PagesTotal);
            IsHidden = false;
        }

        public void RequestUpdateCollection()
        {
            UpdateCollection();
            UpdatePageContent();
            SetPageSizeCommand.Execute(PageSize);
        }

        public void Clear()
        {
            _currentPage = 0;
            Collection.Clear();
            IsHidden = true;
        }

        public void ReadProjectOption(ProjectOptions projectOptions)
        {
            ProjectOptions = projectOptions;
            SetPageSizeCommand.Execute(projectOptions.PageSize);
            SetPageCommand.Execute(projectOptions.LastPage);
            Refresh();
        }

        public void WriteProjectOptions(ProjectOptions projectOptions)
        {
            projectOptions.LastPage = CurrentPage;
            if (IsOtoMode)
            {
                projectOptions.OtoPageSize = PageSize;
            }
            else
            {
                projectOptions.PageSize = PageSize;
            }
        }

        public void SetBase(WavControlViewModel _base)
        {
            if (Base != null)
                Base.PointsChanged -= UpdatePageContent;
            Base = _base;
            Base.PointsChanged += UpdatePageContent;
        }

        public void OtoMode()
        {
            CurrentPage = 0;
            IsOtoMode = true;
            Refresh();
        }

        public void UpdateOtoPreviewControls(List<WavControlBaseViewModel> controls)
        {
            SourceCollection = controls;
            IsHidden = false;
            UpdateCollection();
        }

        public void WaitForPageLoadedAndLoadRest()
        {
            isLoadRestAllowed = true;
            var pageContentLocal = pageContent.ToArray();
            foreach (var control in pageContentLocal)
            {
                var wavControl = control as WavControlViewModel;
                if (wavControl == null)
                    return;
                wavControl.OnLoaded += () =>
                {
                    if (!isLoadRestAllowed)
                        return;
                    foreach (var innerControl in pageContentLocal)
                    {
                        var innerWavControl = control as WavControlViewModel;
                        if (innerWavControl == null || !innerWavControl.IsLoaded)
                            return;
                    }
                    LoadRest();
                };
            }
        }

        public void UnsubscribeBaseChanged()
        {
            if (Base != null)
                Base.PointsChanged -= UpdatePageContent;
        }

        public async void UpdatePageContent()
        {
            SetPageContentReady(false);
            if (!IsHidden)
            {
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
            }
            await Task.Run(() =>
            {
                if (!IsOtoMode)
                    Thread.Sleep(1000);

                SetPageContentReady(true);
            });
        }

        public void Goto(WavControlBaseViewModel model)
        {
            var index = Collection.IndexOf(model);
            if (index >= 0)
            {
                int page = index / PageSize;
                if (page != CurrentPage)
                    SetPageCommand.Execute(page);
            }
        }

        public void RefreshPageContent()
        {
            foreach (var wavconfig in PageContent)
            {
                wavconfig.HandleViewChanged();
            }
        }

        #region private

        private ObservableCollection<WavControlBaseViewModel> pageContent = new ObservableCollection<WavControlBaseViewModel>();
        private int _currentPage = 0;
        private int _pageSize = 7;
        private ProjectOptions ProjectOptions;
        private bool isLoadRestAllowed = false;

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

        private void SetPageContentReady(bool ready)
        {
            foreach (var control in pageContent)
            {
                if (control != Base)
                    control.SetReady(ready);
            }
        }

        private void LoadRest()
        {
            isLoadRestAllowed = false;
            foreach (var control in Collection)
            {
                var wavControl = control as WavControlViewModel;
                if (wavControl == null || wavControl.IsLoaded || wavControl.IsLoading)
                    continue;
                wavControl.LoadExternal();
            }
        }

        private void UpdateCollection()
        {
            Collection.Clear();
            foreach (var control in SourceCollection)
            {
                if (IsOtoMode)
                    Collection.Add(control);
                else
                {
                    if (ProjectOptions != null && ProjectOptions.MustHideNotEnabled && !control.IsEnabled)
                        continue;
                    if (ProjectOptions != null && ProjectOptions.MustHideCompleted && control.IsCompleted)
                        continue;
                    Collection.Add(control);
                }
            }
            Refresh();
            RaisePropertiesChanged(() => Collection, () => CurrentPage);
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
            UpdatePageContent();
        }, currentPage => (currentPage < PagesTotal && currentPage >= 0));

        public ICommand SetPageSizeCommand => new DelegateCommand<int>((pageSize) =>
        {
            var current = _pageSize * _currentPage;
            if (current >= PagesTotal)
                current = PagesTotal - 1;
            _pageSize = pageSize;
            Refresh();
            SetPageCommand.Execute(current / pageSize);
            UpdatePageContent();
            PagerChanged();
        }, pageSize => pageSize > 0 && pageSize != PageSize);

        #endregion
    }
}
