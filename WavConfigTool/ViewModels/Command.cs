using DevExpress.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Input;

namespace WavConfigTool.ViewModels
{
    class DelegateCommand : ICommand
    {
        Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public bool CanExecute(object parameter)
        {
            return this.execute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
    


    class OpenFileCommand : ICommand
    {
        Action<object> execute;
        private Func<object, bool> canExecute;

        public string Filter { get; set; }
        public int FilterIndex { get; set; }

        public string Filename { get; set; }

        public string Title { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public OpenFileCommand(Action<object> execute, string title, string filter, Func<object, bool> canExecute = null)
        {
            this.Title = title;
            this.Filter = filter;
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return this.execute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            var dialog = new OpenFileDialog()
            {
                Title = this.Title,
                Filter = this.Filter
            };
            var result = dialog.ShowDialog();
            if (result.HasValue && result.Value)
            {
                Filename = dialog.FileName;
                this.execute(Filename);
            }
        }

        public bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName)
        {
            throw new NotImplementedException();
        }
    }
}
