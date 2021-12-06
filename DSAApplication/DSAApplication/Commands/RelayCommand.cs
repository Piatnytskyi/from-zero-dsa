using System;
using System.Windows.Input;

namespace DSAApplication.Commands
{
    internal class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;
         
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public RelayCommand(Action<object> command) : this(command, null)
        {

        }

        public RelayCommand(Action<object> command, Predicate<object> canExecute)
        {
            _execute = command ?? throw new ArgumentNullException(nameof(command));
            _canExecute = canExecute;
        }

        public virtual bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public virtual void Execute(object parameter)
        {
            _execute.Invoke(parameter);
        }
    }
}
