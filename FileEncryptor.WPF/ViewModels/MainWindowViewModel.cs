using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
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

        private CancellationTokenSource _ProcessCancellation;

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

        #region ProgressValue : double - Значение прогресса

        /// <summary>Значение прогресса</summary>
        private double _ProgressValue;

        /// <summary>Значение прогресса</summary>
        public double ProgressValue
        {
            get => _ProgressValue;
            set => Set(ref _ProgressValue, value);
        }

        #endregion // Значение прогресса

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

            var progress = new Progress<double>(percent => ProgressValue = percent);

            // Тот кто обладает этим объектом может отменить операцию
            _ProcessCancellation = new CancellationTokenSource();
            var cancel = _ProcessCancellation.Token;

            ((Command) DecryptCommand).Executable = false;
            ((Command) EncryptCommand).Executable = false;
            ((Command)SelectFileCommand).Executable = false;
            try
            {
                await _Encryptor.EncryptAsync(file.FullName, destination_path, Password, Progress: progress,
                    Cancel: cancel);
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancel)
            {
            }
            finally
            {
                _ProcessCancellation.Dispose();
                _ProcessCancellation = null;
            }
            ((Command) EncryptCommand).Executable = true;
            ((Command) DecryptCommand).Executable = true;
            ((Command)SelectFileCommand).Executable = true;

            timer.Stop();

            //_UserDialog.Information("Шифрование", $"Шифрование файла успешно завершено за {timer.Elapsed.TotalSeconds:0.##} с");
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

            var progress = new Progress<double>(percent => ProgressValue = percent);

            _ProcessCancellation = new CancellationTokenSource();
            var cancel = _ProcessCancellation.Token;

            ((Command)DecryptCommand).Executable = false;
            ((Command)EncryptCommand).Executable = false;
            ((Command)SelectFileCommand).Executable = false;
            var decryption_task = _Encryptor.DecryptAsync(file.FullName, destination_path, Password, Progress: progress,
                Cancel: cancel);
            // тут можно расположить код который будет выполнятся параллельно процессу дешифрирования

            var success = false;
            try
            {
                success = await decryption_task;
            }
            catch (OperationCanceledException e) when (e.CancellationToken == cancel)
            {
            }
            finally
            {
                _ProcessCancellation.Dispose();
                _ProcessCancellation = null;
            }
            ((Command)EncryptCommand).Executable = true;
            ((Command)DecryptCommand).Executable = true;
            ((Command)SelectFileCommand).Executable = true;

            timer.Stop();

            if(success)
                _UserDialog.Information("Шифрование", $"Дешифровка файла выполнена успешно за {timer.Elapsed.TotalSeconds:0.##} с");
            else 
                _UserDialog.Warning("Шифрование", "Ошибка при дешифровке файла: указан неверный пароль.");
        }

        #endregion // DecryptCommand

        #region Command : CancelCommand - Отмена операции

        private ICommand _CancelCommand;

        /// <summary>Отмена операции</summary>
        public ICommand CancelCommand => _CancelCommand
            ??= new LambdaCommand(OnCancelCommandExecuted, CanCancelCommandExecute);

        /// <summary>Проверка возможности выполнения - Отмена операции</summary>
        private bool CanCancelCommandExecute(object p) =>
            _ProcessCancellation != null && !_ProcessCancellation.IsCancellationRequested;

        /// <summary>Логика выполнения - Отмена операции</summary>
        private void OnCancelCommandExecuted(object p) => _ProcessCancellation.Cancel();

        #endregion // CancelCommand


        #endregion // Команды

        #region Конструктор

        public MainWindowViewModel(IUserDialog UserDialog, IEncryptor Encryptor)
        {
            _UserDialog = UserDialog;
            _Encryptor = Encryptor;
            Title = "Шифратор";
            Password = "1234";
        }

        #endregion // Конструктор
    }
}
