using System.Windows.Controls;
using System.Collections.Generic; // ��� ������� ������

// ���������, ��� ������������ ���� ����������
namespace NetworkBootManagerUI.Views
{
    public class ClientInfo
    {
        public string? MacAddress { get; set; }
        public string? IpAddress { get; set; }
        public string? Hostname { get; set; }
        public string? GroupName { get; set; } // <-- ���������
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
            // � �������� ���������� ����� ����� �������� ������ �� ���������
        }
    }
}