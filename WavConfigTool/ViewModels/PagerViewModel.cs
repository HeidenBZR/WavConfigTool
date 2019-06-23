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
        private int _pageSize = 4;

        public ObservableCollection<WavControlBaseViewModel> Collection { get; private set; } = new ObservableCollection<WavControlBaseViewModel>();

        // TODO: Проверить корректность
        public int PagesTotal
        {
            // Количество деленное на размер страницы, округляя в большую сторону
            get => Collection.Count() / PageSize + (Collection.Count() % PageSize > 0 ? 1 : 0);
        }
        public int CurrentPage { get => _currentPage; set => SetPageCommand.Execute(value); }
        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }
        public delegate void PagerChangedHandler();
        public event PagerChangedHandler PagerChanged;

        public bool IsHidden { get; set; } = false;

        public int CurrentPageView { get => CurrentPage + 1; set => CurrentPage = value - 1; }

        public ObservableCollection<WavControlBaseViewModel> PageContent
        {
            get
            {
                var pageContent = new ObservableCollection<WavControlBaseViewModel>();
                if (IsHidden)
                    return pageContent;
                for (int i = PageSize * CurrentPage; i < PageSize * CurrentPage + PageSize; i++)
                {
                    if (i < Collection.Count)
                        pageContent.Add(Collection[i]);
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

            PagerChanged += delegate { };

        }

        public void Clear()
        {
            _currentPage = 0;
            Collection.Clear();
            IsHidden = true;
        }

        public ICommand SetFirstPageCommand => new DelegateCommand(delegate
        {
            CurrentPage = 0;
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }, () => PagesTotal > 0);

        public ICommand SetLastPageCommand => new DelegateCommand(delegate
        {
            if (PagesTotal > 0)
                CurrentPage = PagesTotal - 1;
            else
                CurrentPage = 0;
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }, () => PagesTotal > 0);

        public ICommand SetNextPageCommand => new DelegateCommand(delegate
        {
            CurrentPage++;
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }, () => PagesTotal > 0 && CurrentPage < PagesTotal - 1);

        public ICommand SetPreiousPageCommand => new DelegateCommand(delegate
        {
            CurrentPage--;
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
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
            RaisePropertyChanged(() => PageSize);
            RaisePropertyChanged(() => PagesTotal);
            RaisePropertyChanged(() => PageContent);
            PagerChanged();
        }, pageSize => pageSize > 0);

        internal void SetOtoMode(bool isOtoPreviewMode, WavControlBaseViewModel wavControlViewModel)
        {
            throw new NotImplementedException();
        }
    }
}
