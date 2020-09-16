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
using WavConfigTool.UserControls;
using WavConfigTool.ViewModels;
using System.IO;
using System.Diagnostics;
using NAudio.CoreAudioApi;
using System.Runtime.CompilerServices;

namespace WavConfigTool.ViewTools
{
    class DelegateCommonCommand : ICommand
    {
        Action<object> execute;
        private Func<object, bool> canExecute;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public DelegateCommonCommand(Action<object> execute, Func<object, bool> canExecute = null)
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

        public OpenFileCommand(Action<object> execute, string title, string filter, Func<object, bool> canExecute = null, string filename = "")
        {
            this.Title = title;
            this.Filter = filter;
            this.canExecute = canExecute;
            this.Filename = filename;
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
                Filter = this.Filter,
                FileName = Filename
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

    class OpenFolderCommand : DelegateCommand
    {
        public OpenFolderCommand(string dir, bool canBeExecuted = true) : base(() =>
        {
            Process.Start(new ProcessStartInfo()
            {
                FileName = dir,
                UseShellExecute = true,
                Verb = "open"
            });
        }, Directory.Exists(dir) && canBeExecuted)
        {

        }
    }
    class SaveFileCommand : ICommand
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

        public SaveFileCommand(Action<object> execute, string title, string filter, Func<object, bool> canExecute = null, string filename = "")
        {
            this.Title = title;
            this.Filter = filter;
            this.canExecute = canExecute;
            this.Filename = filename;
            this.execute = execute;
        }

        public bool CanExecute(object parameter)
        {
            return this.execute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            var dialog = new SaveFileDialog()
            {
                Title = this.Title,
                Filter = this.Filter,
                FileName = Filename
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

    class MessageBoxCommand : ICommand
    {
        Action<object> execute;
        private Func<object, bool> canExecute;

        public int FilterIndex { get; set; }

        public string Text { get; set; }

        public MessageBoxButton MessageBoxButton { get; set; }
        public MessageBoxImage MessageBoxImage { get; set; }

        public string Title { get; set; }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public MessageBoxCommand(Action<object> execute, string title, string text, Func<object, bool> canExecute = null)
        {
            this.Title = title;
            this.Text = text;
            this.canExecute = canExecute;
            this.execute = execute;
            this.MessageBoxImage = MessageBoxImage.Information;
            this.MessageBoxButton = MessageBoxButton.OK;
        }

        public bool CanExecute(object parameter)
        {
            return this.execute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            var result = MessageBox.Show(Text, Title, MessageBoxButton, MessageBoxImage);
            if (result == MessageBoxResult.Yes)
            {
                this.execute(result);
            }
        }

        public bool ShowDialog(Action<CancelEventArgs> fileOK, string directoryName)
        {
            throw new NotImplementedException();
        }
    }

    class MessageBoxConfirmationCommand : MessageBoxCommand
    {
        public MessageBoxConfirmationCommand(Action<object> execute, string title, string text, Func<object, bool> canExecute = null) : base(execute, title, text, canExecute)
        {
            this.MessageBoxImage = MessageBoxImage.Question;
            this.MessageBoxButton = MessageBoxButton.YesNo;
        }
    }
}
