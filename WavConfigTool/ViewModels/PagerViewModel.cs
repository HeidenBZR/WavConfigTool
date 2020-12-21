using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using WavConfigCore;
using WavConfigTool.Classes;

namespace WavConfigTool.ViewModels
{
    public class PagerViewModel : ViewModelBase
    {
        public ProjectLineContainer Base { get; set; }

        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }
        public int ItemsCount => Collection != null ? Collection.Count : 0;
        public int PagesTotal => GetPagesTotal();
        public int CurrentPage
        {
            get => _currentPage;
            set => SetPageCommand.Execute(value);
        }
        public int CurrentPageView => CurrentPage + 1;
        public bool IsHidden { get; set; } = false;
        public bool IsOtoMode { get; set; } = false;
        public ImagesLibrary ImagesLibrary;

        public List<PagerContentBase> SourceCollection { get; private set; } = new List<PagerContentBase>();
        public List<PagerContentBase> Collection { get; private set; } = new List<PagerContentBase>();
        public List<WavControlBaseViewModel> PageContent { get; private set; }

        public event SimpleHandler PagerChanged = delegate { };

        public ViewOptions ViewOptions { get; private set; }


        public PagerViewModel()
        {
            RaisePropertyChanged(() => PageContent);
            IsHidden = false;
        }

        public PagerViewModel(List<PagerContentBase> collection, ViewOptions viewOptions, ImagesLibrary imagesLibrary)
        {
            ViewOptions = viewOptions;
            ImagesLibrary = imagesLibrary;
            PageContent = new List<WavControlBaseViewModel>();
            SourceCollection = collection;
            ProjectLineContainer firstContainer = null;
            foreach (var content in collection)
            {
                var container = content as ProjectLineContainer;
                if (container == null)
                    break;
                if (firstContainer == null)
                    firstContainer = container;
                ImagesLibrary.RegisterWaveForm(container.WaveForm);
            }
            UpdateCollection();
            RaisePropertyChanged(() => PageContent);
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PagesTotal);
            IsHidden = false;

            var spectrogram = new Spectrogram();
            spectrogram.Start(firstContainer.WaveForm);
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

        public void SetBase(ProjectLineContainer _base)
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

        public void UpdateOtoPreviewControls(List<PagerContentBase> controls)
        {
            SourceCollection = controls;
            IsHidden = false;
            UpdateCollection();
            UpdatePageContent();
        }

        public void WaitForPageLoadedAndLoadRest()
        {
            isLoadRestAllowed = true;
            var PageContentLocal = PageContent.ToArray();
            foreach (var control in PageContentLocal)
            {
                var wavControl = control as WavControlViewModel;
                if (wavControl == null)
                    return;
                wavControl.OnLoaded += () =>
                {
                    if (!isLoadRestAllowed)
                        return;
                    foreach (var innerControl in PageContentLocal)
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
        public void Goto(PagerContentBase model)
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

        private int _currentPage = 0;
        private int _pageSize = 7;
        private ProjectOptions ProjectOptions;
        private bool isLoadRestAllowed = false;
        private readonly int WavImageHeight = 100;

        private void UpdatePageContent()
        {
            SetPageContentReady(false);
            if (!IsHidden)
            {
                if (IsOtoMode)
                {
                    if (PageContent.Count == 0)
                    {
                        var model = new WavControlViewModel();
                        InitWavControlBase(model);
                        PageContent.Add(model);
                        model.Update(Base);
                    }
                    else if (PageContent[0].PagerContent != Base)
                    {
                        PageContent[0].Update(Base);
                    }
                    while (PageContent.Count >= PageSize)
                    {
                        PageContent.RemoveAt(PageContent.Count - 1);
                    }
                    while (PageContent.Count < PageSize)
                    {
                        var model = new OtoPreviewControlViewModel();
                        InitWavControlBase(model);
                        PageContent.Add(model);
                    }
                    var otosPageSize = PageSize - 1;
                    for (int i = 0; otosPageSize * CurrentPage + i < otosPageSize * (CurrentPage + 1) && otosPageSize * CurrentPage + i < Collection.Count; i++)
                    {
                        var collectionI = otosPageSize * CurrentPage + i;
                        PageContent[i + 1].Update(Collection[collectionI]);
                    }
                }
                else
                {
                    while (PageContent.Count > PageSize)
                    {
                        PageContent.RemoveAt(PageContent.Count - 1);
                    }
                    while (PageContent.Count < PageSize)
                    {
                        var model = new WavControlViewModel();
                        InitWavControlBase(model);
                        PageContent.Add(model);
                    }
                    for (int i = 0; PageSize * CurrentPage + i < PageSize * (CurrentPage + 1) && PageSize * CurrentPage + i < Collection.Count; i++)
                    {
                        PageContent[i].Update(Collection[PageSize * CurrentPage + i]);
                    }
                }
            }
            //await Task.Run(() =>
            //{
            //    if (!IsOtoMode)
            //        Thread.Sleep(1000);

            //    SetPageContentReady(true);
            //});

            SetPageContentReady(true);
            RaisePropertyChanged(nameof(PageContent));
        }

        private void InitWavControlBase(WavControlBaseViewModel model)
        {
            model.ImagesLibrary = ImagesLibrary;
            model.Height = WavImageHeight;
            model.ViewOptions = ViewOptions;
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

        private void SetPageContentReady(bool ready)
        {
            foreach (var control in PageContent)
            {
                control.SetReady(ready);
            }
        }

        private void LoadRest()
        {
            isLoadRestAllowed = false;
            foreach (var control in Collection)
            {
                var container = control as ProjectLineContainer;
                if (container == null || container.IsLoadingImages || container.IsLoadingImages)
                    continue;
                container.LoadImages(WavImageHeight);
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
