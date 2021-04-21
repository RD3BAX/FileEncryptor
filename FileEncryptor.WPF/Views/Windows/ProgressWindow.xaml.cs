using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;

namespace FileEncryptor.WPF.Views.Windows
{
    /// <summary>
    /// Логика взаимодействия для ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow
    {

        #region Свойства

        #region ProgressInformer

        private IProgress<double> _ProgressInformer;

        public IProgress<double> ProgressInformer => _ProgressInformer ??= new Progress<double>(p => ProgressValue = p);

        #endregion // ProgressInformer

        #region StatusInformer

        private IProgress<string> _StatusInformer;

        public IProgress<string> StatusInformer => _StatusInformer ??= new Progress<string>(status => Status = status);

        #endregion // StatusInformer

        #region ProgressStatusInformer

        private IProgress<(double Percent, string Message)> _ProgressStatusInformer;

        public IProgress<(double Percent, string Message)> ProgressStatusInformer => _ProgressStatusInformer
            ??= new Progress<(double Percent, string Message)>(
                p =>
                {
                    ProgressValue = p.Percent;
                    Status = p.Message;
                });

        #endregion // ProgressStatusInformer

        #region Cancel

        private CancellationTokenSource _Cancellation;

        public CancellationToken Cancel
        {
            get
            {
                if (_Cancellation != null) return _Cancellation.Token;
                _Cancellation = new CancellationTokenSource();
                CancellButton.IsEnabled = true;
                return _Cancellation.Token;
            }
        }

        #endregion // Cancel

        #region Status : string - Статусное сообщение

        /// <summary>Статусное сообщение</summary>
        public static readonly DependencyProperty StatusProperty =
            DependencyProperty.Register(
                nameof(Status),
                typeof(string),
                typeof(ProgressWindow),
                new PropertyMetadata(default(string)));

        /// <summary>Статусное сообщение</summary>
        //[Category("")]
        [Description("Статусное сообщение")]
        public string Status
        {
            get => (string) GetValue(StatusProperty);
            set => SetValue(StatusProperty, value);
        }

        #endregion // Status

        #region ProgressValue : double - Значение прогресса

        /// <summary>Значение прогресса</summary>
        public static readonly DependencyProperty ProgressValueProperty =
            DependencyProperty.Register(
                nameof(ProgressValue),
                typeof(double),
                typeof(ProgressWindow),
                new PropertyMetadata(double.NaN, OnProgressChanged));

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var progress_value = (double) e.NewValue;
            var progress_view = ((ProgressWindow) d).ProgressView;
            progress_view.Value = progress_value;
            progress_view.IsIndeterminate = double.IsNaN(progress_value);
        }

        /// <summary>Значение прогресса</summary>
        //[Category("")]
        [Description("Значение прогресса")]
        public double ProgressValue
        {
            get => (double) GetValue(ProgressValueProperty);
            set => SetValue(ProgressValueProperty, value);
        }

        #endregion // ProgressValue

        #endregion // Свойства

        public ProgressWindow() => InitializeComponent();

        private void OnCancellClick(object sender, RoutedEventArgs e)
        {
            _Cancellation?.Cancel();
        }
    }
}
