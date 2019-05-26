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

        public ObservableCollection<WavControlViewModel> Collection { get; private set; } = new ObservableCollection<WavControlViewModel>();

        // TODO: Проверить корректность
        public int PagesTotal
        {
            // Количество деленное на размер страницы, округляя в большую сторону
            get => Collection.Count() / PageSize + (Collection.Count() % PageSize > 0 ? 1 : 0);
        }
        public int CurrentPage { get => _currentPage; set => SetPageCommand.Execute(value); }
        public int PageSize { get => _pageSize; set => SetPageSizeCommand.Execute(value); }



        public int CurrentPageView { get => CurrentPage + 1; set => CurrentPage = value - 1; }

        public ObservableCollection<WavControlViewModel> PageContent
        {
            get
            {
                var pageContent = new ObservableCollection<WavControlViewModel>();
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
            RaisePropertyChanged(() => PageContent);
        }

        public PagerViewModel(ObservableCollection<WavControlViewModel> collection)
        {
            Collection = collection;
            RaisePropertyChanged(() => PageContent);
            RaisePropertyChanged(() => CurrentPageView);
            RaisePropertyChanged(() => PagesTotal);

            
        }

        public ICommand SetFirstPageCommand
        {
            get
            {
                return new DelegateCommand(delegate                
                {
                    CurrentPage = 0;
                    RaisePropertyChanged(() => CurrentPageView);
                    RaisePropertyChanged(() => PageContent);
                }, () => PagesTotal > 0);
            }
        }

        public ICommand SetLastPageCommand
        {
            get
            {
                return new DelegateCommand(delegate
                {
                    if (PagesTotal > 0)
                        CurrentPage = PagesTotal - 1;
                    else
                        CurrentPage = 0;
                    RaisePropertyChanged(() => CurrentPageView);
                    RaisePropertyChanged(() => PageContent);
                }, () => PagesTotal > 0);
            }
        }
        public ICommand SetNextPageCommand
        {
            get
            {
                return new DelegateCommand(delegate
                {
                    CurrentPage++;
                    RaisePropertyChanged(() => CurrentPageView);
                    RaisePropertyChanged(() => PageContent);
                }, () =>  PagesTotal > 0 && CurrentPage < PagesTotal - 1 );
            }
        }

        public ICommand SetPreiousPageCommand
        {
            get
            {
                return new DelegateCommand(delegate
                {
                    CurrentPage--;
                    RaisePropertyChanged(() => CurrentPageView);
                    RaisePropertyChanged(() => PageContent);
                }, () => PagesTotal > 0 && CurrentPage > 0);
            }
        }

        public ICommand SetPageCommand
        {
            get
            {
                return new DelegateCommonCommand((obj) =>
                {
                    _currentPage = (int)obj;
                    RaisePropertyChanged(() => CurrentPageView);
                    RaisePropertyChanged(() => PageContent);
                }, param => (
                    param is int &&
                    (int)param < PageSize &&
                    (int)param >= 0));
            }
        }

        public ICommand SetPageSizeCommand
        {
            get
            {
                return new DelegateCommonCommand((obj) =>
                {
                    _pageSize = (int)obj;
                    // TODO: Проверка на допустимость страницы? 
                    // TODO: Переход к странице с объектом на прежней странице
                    RaisePropertyChanged(() => PageSize);
                    RaisePropertyChanged(() => PagesTotal);
                    RaisePropertyChanged(() => PageContent);
                }, param => (
                    param is int &&
                    (int)param > 0));
            }
        }

    }
}
