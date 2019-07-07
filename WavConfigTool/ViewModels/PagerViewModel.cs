using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using WavConfigTool.ViewTools;

namespace WavConfigTool.ViewModels
{
    public class PagerViewModel : ViewModelBase
    {
        private int _currentPage = 0;
        private int _pageSize = 7;

        public ObservableCollection<WavControlBaseViewModel> Collection { get; private set; } = new ObservableCollection<WavControlBaseViewModel>();

        public int ItemsCount { get; set; } = 0;

        // TODO: Проверить корректность
        public int PagesTotal
        {
            // Количество деленное на размер страницы, округляя в большую сторону
            get => ItemsCount / PageSize + (ItemsCount % PageSize > 0 ? 1 : 0);
        }
        public int CurrentPage { get => _currentPage; set => SetPageCommand.Execute(value); }
        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }
        public delegate void SimpleHandler();
        public event SimpleHandler PagerChanged;

        public WavControlBaseViewModel Base { get; set; }
        public bool IsHidden { get; set; } = false;

        public bool IsOtoMode { get; set; } = false;
        public int CurrentPageView { get => CurrentPage + 1; set => CurrentPage = value - 1; }

        public ObservableCollection<WavControlBaseViewModel> PageContent
        {
            get
            {
                var pageContent = new ObservableCollection<WavControlBaseViewModel>();
                if (IsHidden)
                    return pageContent;
                if (IsOtoMode)
                {
                    pageContent.Add(Base);
                    var otosPageSize = PageSize - 1;
                    for (int i = otosPageSize * CurrentPage; i < otosPageSize * (CurrentPage + 1)  && i < Collection.Count; i++)
                    {
                        pageContent.Add(Collection[i]);
                    }
                }
                else
                {
                    for (int i = PageSize * CurrentPage; i < PageSize * (CurrentPage + 1); i++)
                    {
                        if (i < Collection.Count)
                            pageContent.Add(Collection[i]);
                    }
                }
                return pageContent;
            }
        }

        public PagerViewModel()
        {
            PagerChanged += delegate { };
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
            PagerChanged += delegate { };

        }

        public void Clear()
        {
            _currentPage = 0;
            Collection.Clear();
            IsHidden = true;
        }

        public void ReadProjectOption(string key, string value)
        {
            switch (key)
            {
                case "Pager.PageSize":
                    if (int.TryParse(value, out int pageSize))
                    {
                        SetPageSizeCommand.Execute(pageSize);
                    }
                    break;
                case "Pager.CurrentPage":
                    if (int.TryParse(value, out int page))
                    {
                        SetPageCommand.Execute(page);
                    }
                    break;
            }
        }

        public Dictionary<string, string> WriteProjectOptions()
        {
            return new Dictionary<string, string>
            {
                ["Pager.CurrentPage"] = CurrentPage.ToString(),
                ["Pager.PageSize"] = PageSize.ToString()
            };
        }

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
        }, currentPage => (currentPage < PageSize && currentPage >= 0));

        public ICommand SetPageSizeCommand => new DelegateCommand<int>((pageSize) =>
        {
            _pageSize = pageSize;
            // TODO: Проверка на допустимость страницы? 
            // TODO: Переход к странице с объектом на прежней странице
            Refresh();
        }, pageSize => pageSize > 0);

        internal void OtoMode()
        {
            CurrentPage = 0;
            IsOtoMode = true;
            Refresh();
        }

        void Refresh()
        {
            RaisePropertyChanged(() => PageSize);
            RaisePropertyChanged(() => PagesTotal);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }

        internal void UpdateOtoPreviewControls(ObservableCollection<WavControlBaseViewModel> controls)
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
    }
}
