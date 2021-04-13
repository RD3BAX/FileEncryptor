using FileEncryptor.WPF.ViewModels.Base;


namespace FileEncryptor.WPF.ViewModels
{
    internal class MainWindowViewModel : ViewModel
    {
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

        #endregion // Свойства

        #region Конструктор

        public MainWindowViewModel()
        {
            Title = "Шифратор";
        }

        #endregion // Конструктор
    }
}
