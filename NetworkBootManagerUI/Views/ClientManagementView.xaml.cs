using System.Windows.Controls;
using System.Collections.Generic; // Для примера данных

// Убедитесь, что пространство имен правильное
namespace NetworkBootManagerUI.Views
{
    public class ClientInfo
    {
        public string? MacAddress { get; set; }
        public string? IpAddress { get; set; }
        public string? Hostname { get; set; }
        public string? GroupName { get; set; } // <-- ДОБАВЛЕНО
        public string? Status { get; set; }
        public string? AssignedImage { get; set; }
        public bool IsEnabled { get; set; }
        public System.DateTime DiscoveryTime { get; set; } = System.DateTime.Now;
    }


    /// <summary>
    /// Interaction logic for ClientManagementView.xaml
    /// </summary>
    public partial class ClientManagementView : UserControl
    {
        public ClientManagementView()
        {
            InitializeComponent();
            // В реальном приложении здесь будет загрузка данных из источника
        }
    }
}