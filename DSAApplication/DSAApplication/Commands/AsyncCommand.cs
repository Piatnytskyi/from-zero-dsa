using DSAApplication.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace DSAApplication.Commands
{
    internal class AsyncCommand : ICommand
    {
        private bool _isExecuting;

        private readonly Func<object, Task> _execute;
        private readonly Predicate<object> _canExecute;

        private readonly IErrorHandler _errorHandler;

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public AsyncCommand(Func<object, Task> command, IErrorHandler errorHandler) : this(command, null, errorHandler)
        {

        }

        public AsyncCommand(
            Func<object, Task> command,
            Predicate<object> canExecute = null,
            IErrorHandler errorHandler = null)
        {
            _execute = command;
            _canExecute = canExecute;
            _errorHandler = errorHandler;
        }

        public bool CanExecute(object parameter)
        {
            return !_isExecuting && (_canExecute?.Invoke(parameter) ?? true);
        }

        public async Task ExecuteAsync(object parameter)
        {
            if (CanExecute(parameter))
            {
                try
                {
                    _isExecuting = true;
                    await _execute(parameter);
                }
                finally
                {
                    _isExecuting = false;
                }
            }
        }

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        void ICommand.Execute(object parameter)
        {
            ExecuteAsync(parameter).FireAndForgetSafeAsync(_errorHandler);
        }
    }
}
