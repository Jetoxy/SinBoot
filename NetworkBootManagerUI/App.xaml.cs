using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading; // <-- ��������
using System.Diagnostics;      // <-- �������� ��� Debug.WriteLine

namespace NetworkBootManagerUI
{
    public partial class App : Application
    {
        // �������������� ����� OnStartup ��� ��������� ����������� App()
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupGlobalExceptionHandling();
        }

        // ��� �������� ��� � �����������, ���� ����������� ���:
        // public App()
        // {
        //     SetupGlobalExceptionHandling();
        // }


        private void SetupGlobalExceptionHandling()
        {
            // ��������� ���������� � �������� UI-������
            DispatcherUnhandledException += (s, e) =>
            {
                string exceptionMessage = $"UI Thread Exception:\n{e.Exception}";
                Debug.WriteLine(exceptionMessage);
                MessageBox.Show(exceptionMessage, "Unhandled UI Exception", MessageBoxButton.OK, MessageBoxImage.Error);
                e.Handled = true; // �������� ��� ������������, ����� ���������� �� ��������� ����� (��� �������)
                // � �������� ������ ����� ������, ��������� �� ����������:
                // Current.Shutdown(-1);
            };

            // ��������� ���������� � ������ ������� (�� UI)
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                string exceptionMessage = $"Background Thread Exception:\n{e.ExceptionObject}";
                 Debug.WriteLine(exceptionMessage);
                 // MessageBox ����� ���������� �����������, �.�. ��� �� UI-�����
                 // ����� �������� � ���-����
                 // Environment.Exit(1); // ������������� ��������� ����������, ���� �����
            };
        }
    }
}