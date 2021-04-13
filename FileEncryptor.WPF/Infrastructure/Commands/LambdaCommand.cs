using System;
using FileEncryptor.WPF.Infrastructure.Commands.Base;

namespace FileEncryptor.WPF.Infrastructure.Commands
{
    class LambdaCommand : Command
    {
        #region Поля

        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        #endregion // Поля

        #region Методы

        protected override bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        protected override void Execute(object parameter) => _execute(parameter);

        #endregion // Методы

        #region Конструктор

        public LambdaCommand(Action Execute, Func<bool> CanExecute = null) 
            : this(
                Execute: p => Execute(), 
                CanExecute: CanExecute is null ? (Func<object, bool>)null : p => CanExecute())
        {
            
        }

        public LambdaCommand(Action<object> Execute, Func<object, bool> CanExecute = null)
        {
            _execute = Execute ?? throw new ArgumentNullException(nameof(Execute));
            _canExecute = CanExecute;
        }

        #endregion // Конструктор
    }
}
