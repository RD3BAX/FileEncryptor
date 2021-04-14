using System.IO;
using System.Windows.Input;
using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;


namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Поля

        private readonly IUserDialog _UserDialog;

        #endregion // Поля

        #region Свойства

        #region Title : string - Заголовок окна

        /// <summary>Заголовок окна</summary>
        private string _Title;

        /// <summary>Заголовок окна</summary>
        public string Title
        {
            get => _Title;
            set => Set(ref _Title, value);
        }

        #endregion // Заголовок окна

        #region Password : string - Пароль

        /// <summary>Пароль</summary>
        private string _Password;

        /// <summary>Пароль</summary>
        public string Password
        {
            get => _Password;
            set => Set(ref _Password, value);
        }

        #endregion // Пароль

        #region SelectedFile : FileInfo - Выбранный файл

        /// <summary>Выбранный файл</summary>
        private FileInfo _SelectedFile;

        /// <summary>Выбранный файл</summary>
        public FileInfo SelectedFile
        {
            get => _SelectedFile;
            set => Set(ref _SelectedFile, value);
        }

        #endregion // Выбранный файл

        #endregion // Свойства

        #region Команды

        #region Command : SelectFileCommand - Выбрать файл

        private ICommand _SelectFileCommand;

        /// <summary>Выбрать файл</summary>
        public ICommand SelectFileCommand => _SelectFileCommand
            ??= new LambdaCommand(OnSelectFileCommandExecuted/*, CanSelectFileCommandExecute*/);

        ///// <summary>Проверка возможности выполнения - Выбрать файл</summary>
        //private bool CanSelectFileCommandExecute(object p) => true;

        /// <summary>Логика выполнения - Выбрать файл</summary>
        private void OnSelectFileCommandExecuted(object p)
        {
            if (!_UserDialog.OpenFile("Выбор файла для шифрования", out var file_path)) return;
            var selected_file = new FileInfo(file_path);
            SelectedFile = selected_file.Exists ? selected_file : null;
        }

        #endregion // SelectFileCommand


        #endregion // Команды

        #region Конструктор

        public MainWindowViewModel(IUserDialog UserDialog)
        {
            _UserDialog = UserDialog;
            Title = "Шифратор";
        }

        #endregion // Конструктор
    }
}
