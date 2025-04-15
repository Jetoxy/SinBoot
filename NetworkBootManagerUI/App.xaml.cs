using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading; // <-- Добавить
using System.Diagnostics;      // <-- Добавить для Debug.WriteLine

namespace NetworkBootManagerUI
{
    public partial class App : Application
    {
        // Переопределяем метод OnStartup или добавляем конструктор App()
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupGlobalExceptionHandling();
        }

        // Или добавьте это в конструктор, если используете его:
        // public App()
        // {
        //     SetupGlobalExceptionHandling();
        // }


        private void SetupGlobalExceptionHandling()
        {
            // Обработка исключений в основном UI-потоке
            DispatcherUnhandledException += (s, e) =>
            {
                string exceptionMessage = $"UI Thread Exception:\n{e.Exception}";
                Debug.WriteLine(exceptionMessage);
                MessageBox.Show(exceptionMessage, "Unhandled UI Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true; // Пометить как обработанное, чтобы приложение НЕ закрылось сразу (для отладки)
                // В релизной версии можно решить, закрывать ли приложение:
                // Current.Shutdown(-1);
            };

            // Обработка исключений в других потоках (не UI)
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                string exceptionMessage = $"Background Thread Exception:\n{e.ExceptionObject}";
                 Debug.WriteLine(exceptionMessage);
                 // MessageBox здесь показывать небезопасно, т.к. это не UI-поток
                 // Лучше записать в лог-файл
                 // Environment.Exit(1); // Принудительно завершить приложение, если нужно
            };
        }
    }
}