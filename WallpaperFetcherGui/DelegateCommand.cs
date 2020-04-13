using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace WallpaperFetcherGui
{
    public class DelegateCommand : ICommand
    {
        private Action _execute;
        private Func<bool> _canExecute;

        public DelegateCommand(Action execute)
        {
            _execute = execute ?? (() => { });
            _canExecute = () => true;
        }
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return _canExecute();
        }

        public void Execute(object parameter)
        {
            _execute();
        }
    }
}
