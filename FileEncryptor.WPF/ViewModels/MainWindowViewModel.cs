using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using FileEncryptor.WPF.Infrastructure.Commands;
using FileEncryptor.WPF.Infrastructure.Commands.Base;
using FileEncryptor.WPF.Services.Interfaces;
using FileEncryptor.WPF.ViewModels.Base;


namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
        #region Поля

        private const string __EncryptedFileSuffix = ".encrypted";

        private readonly IUserDialog _UserDialog;
        private readonly IEncryptor _Encryptor;

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

        #region Command : EncryptCommand - Зашифровать

        private ICommand _EncryptCommand;

        /// <summary>Зашифровать</summary>
        public ICommand EncryptCommand => _EncryptCommand
            ??= new LambdaCommand(OnEncryptCommandExecuted, CanEncryptCommandExecute);

        /// <summary>Проверка возможности выполнения - Зашифровать</summary>
        private bool CanEncryptCommandExecute(object p) =>
            (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>Логика выполнения - Зашифровать</summary>
        private async void OnEncryptCommandExecuted(object p)
        {
            var file = p as FileInfo ?? SelectedFile;
            if (file is null) return;

            var default_file_name = file.FullName + __EncryptedFileSuffix;
            if (!_UserDialog.SaveFile("Выбор файла для сохранения", out var destination_path, default_file_name)) return;

            var timer = Stopwatch.StartNew();

            ((Command) EncryptCommand).Executable = false;
            await _Encryptor.EncryptAsync(file.FullName, destination_path, Password);
            ((Command)EncryptCommand).Executable = true;

            timer.Stop();

            _UserDialog.Information("Шифрование", $"Шифрование файла успешно завершено за {timer.Elapsed.TotalSeconds:0.##} с");
        }

        #endregion // EncryptCommand

        #region Command : DecryptCommand - Расшифровать

        private ICommand _DecryptCommand;

        /// <summary>Расшифровать</summary>
        public ICommand DecryptCommand => _DecryptCommand
            ??= new LambdaCommand(OnDecryptCommandExecuted, CanDecryptCommandExecute);

        /// <summary>Проверка возможности выполнения - Расшифровать</summary>
        private bool CanDecryptCommandExecute(object p) =>
            (p is FileInfo file && file.Exists || SelectedFile != null) && !string.IsNullOrWhiteSpace(Password);

        /// <summary>Логика выполнения - Расшифровать</summary>
        private async void OnDecryptCommandExecuted(object p)
        {
            var file = p as FileInfo ?? SelectedFile;
            if (file is null) return;

            var default_file_name = file.FullName.EndsWith(__EncryptedFileSuffix)
                ? file.FullName.Substring(0, file.FullName.Length - __EncryptedFileSuffix.Length)
                : file.FullName;
            if (!_UserDialog.SaveFile("Выбор файла для сохранения", out var destination_path, default_file_name)) return;

            var timer = Stopwatch.StartNew();

            ((Command)DecryptCommand).Executable = false;
            var decryption_task = _Encryptor.DecryptAsync(file.FullName, destination_path, Password);
            // тут можно расположить код который будет выполнятся параллельно процессу дешифрирования
            var success = await decryption_task;
            ((Command)DecryptCommand).Executable = true;

            timer.Stop();

            if(success)
                _UserDialog.Information("Шифрование", $"Дешифровка файла выполнена успешно за {timer.Elapsed.TotalSeconds:0.##} с");
            else 
                _UserDialog.Warning("Шифрование", "Ошибка при дешифровке файла: указан неверный пароль.");
        }

        #endregion // DecryptCommand

        #endregion // Команды

        #region Конструктор

        public MainWindowViewModel(IUserDialog UserDialog, IEncryptor Encryptor)
        {
            _UserDialog = UserDialog;
            _Encryptor = Encryptor;
            Title = "Шифратор";
        }

        #endregion // Конструктор
    }
}
