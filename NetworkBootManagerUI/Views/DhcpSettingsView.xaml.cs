using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Linq;

namespace NetworkBootManagerUI.Views
{
    /// <summary>
    /// Interaction logic for DhcpSettingsView.xaml
    /// </summary>
    public partial class DhcpSettingsView : UserControl
    {
        private const string SETTINGS_FILE_PATH = "DhcpSettings.json";
        private DhcpSettings _settings;
        private List<NetworkInterface> _availableInterfaces;

        public DhcpSettingsView()
        {
            InitializeComponent();
            
            // ������������� ��������
            _settings = new DhcpSettings();
            
            // ��������� ��������� ������� �����������
            _availableInterfaces = GetNetworkInterfaces();
            
            // ���������� ���������� �������� ������������
            PopulateNetworkInterfaces();
            
            // �������� �������� ��� �������
            LoadSettings();
            
            // �������� ����������� ������� � ������ ����������
            // ������� ������ "��������� ��������� DHCP" � ������ StackPanel
            StackPanel? buttonPanel = FindStackPanelWithButtons();
            Button? saveButton = null;
            Button? startButton = null;
            Button? stopButton = null;
            
            if (buttonPanel != null)
            {
                foreach (var child in buttonPanel.Children)
                {
                    if (child is Button button)
                    {
                        string content = button.Content?.ToString() ?? "";
                        if (content == "��������� ��������� DHCP")
                            saveButton = button;
                        else if (content == "��������� ������")
                            startButton = button;
                        else if (content == "���������� ������")
                            stopButton = button;
                    }
                }
            }
            
            if (saveButton != null)
            {
                saveButton.Click += SaveSettings_Click;
            }

            if (startButton != null)
            {
                startButton.Click += StartServer_Click;
            }

            if (stopButton != null)
            {
                stopButton.Click += StopServer_Click;
            }

            // ���������� ������������ ��� �����������
            rbInternalDhcp.Checked += RbDhcpMode_Checked;
            rbExternalDhcp.Checked += RbDhcpMode_Checked;
        }

        // ���������� ��� ������������ ������� DHCP
        private void RbDhcpMode_Checked(object sender, RoutedEventArgs e)
        {
            // ��� �������� ��� ��������� � XAML ����� IsEnabled="{Binding ElementName=rbInternalDhcp, Path=IsChecked}"
            // �� ����� �������� �������������� ������ ��� �������������
        }

        // ��������� ������ ������� �����������
        public List<NetworkInterface> GetNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up 
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();
        }
        
        // ��������� IP-������ ����������
        public IPAddress? GetInterfaceIpAddress(NetworkInterface networkInterface)
        {
            var ipProps = networkInterface.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses
                .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            
            return ipv4?.Address;
        }

        // ���������� ���������� �������� ������������
        private void PopulateNetworkInterfaces()
        {
            ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
            if (networkComboBox == null) return;

            networkComboBox.Items.Clear();
            
            // ���������� ��������������� ������
            networkComboBox.Items.Add(new ComboBoxItem { Content = "(�������������)" });
            
            // ���������� ���� ��������� �����������
            foreach (var iface in _availableInterfaces)
            {
                var ipAddress = GetInterfaceIpAddress(iface);
                string displayName = $"{iface.Name} - {ipAddress}";
                networkComboBox.Items.Add(new ComboBoxItem { Content = displayName, Tag = iface });
            }
            
            // ����� ������� �������� �� ���������
            networkComboBox.SelectedIndex = 0;
        }

        // ����������� ��� ������ ������� � ��������� �������
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ������ ������� DHCP �������
                // ����� �� ������ ������������ _settings � _availableInterfaces
                
                MessageBox.Show("DHCP ������ �������", "����������", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ������� DHCP �������: {ex.Message}", "������", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ������ ��������� DHCP �������
                
                MessageBox.Show("DHCP ������ ����������", "����������", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ��� ��������� DHCP �������: {ex.Message}", "������", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // ����� ��� �������� �������� DHCP
        public class DhcpSettings
        {
            // ����� ���������
            public bool UseInternalDhcp { get; set; } = true;
            
            // ��������� �������� DHCP
            public string ExternalDhcpServer { get; set; } = "";
            
            // ��������� ����������� DHCP
            public string NetworkInterface { get; set; } = "(�������������)";
            public string StartIpAddress { get; set; } = "192.168.1.100";
            public string EndIpAddress { get; set; } = "192.168.1.200";
            public string SubnetMask { get; set; } = "255.255.255.0";
            public string Gateway { get; set; } = "";
            public string DnsServer1 { get; set; } = "";
            public string DnsServer2 { get; set; } = "";
            public int LeaseTime { get; set; } = 120;
            public string HostnamePrefix { get; set; } = "";
        }
        
        // �������� �������� �� �����
        private void LoadSettings()
        {
            try
            {
                if (File.Exists(SETTINGS_FILE_PATH))
                {
                    string json = File.ReadAllText(SETTINGS_FILE_PATH);
                    var loadedSettings = JsonSerializer.Deserialize<DhcpSettings>(json);
                    
                    if (loadedSettings != null)
                    {
                        _settings = loadedSettings;
                        // ���������� �������� � ��������� ����������
                        ApplySettingsToUI();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ �������� ��������: {ex.Message}", "������", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // ���������� ����������� �������� � ����������
        private void ApplySettingsToUI()
        {
            // ����� ������ DHCP
            rbInternalDhcp.IsChecked = _settings.UseInternalDhcp;
            rbExternalDhcp.IsChecked = !_settings.UseInternalDhcp;
            
            // ��������� �������� DHCP
            TextBox? externalDhcpTextBox = FindTextBoxInGroupBox(gbExternalSettings);
            if (externalDhcpTextBox != null)
                externalDhcpTextBox.Text = _settings.ExternalDhcpServer;
            
            // ��������� ����������� DHCP - ���������
            ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
            if (networkComboBox != null)
            {
                foreach (ComboBoxItem item in networkComboBox.Items)
                {
                    if (item.Content?.ToString() == _settings.NetworkInterface)
                    {
                        networkComboBox.SelectedItem = item;
                        break;
                    }
                }
            }
            
            // ��������� ��������� ���� � gbInternalSettings
            var textBoxes = FindTextBoxesInGroupBox(gbInternalSettings);
            int index = 0;
            foreach (var textBox in textBoxes)
            {
                switch (index)
                {
                    case 0: // ��������� IP �����
                        textBox.Text = _settings.StartIpAddress;
                        break;
                    case 1: // �������� IP �����
                        textBox.Text = _settings.EndIpAddress;
                        break;
                    case 2: // ����� �������
                        textBox.Text = _settings.SubnetMask;
                        break;
                    case 3: // ����
                        textBox.Text = _settings.Gateway;
                        break;
                    case 4: // DNS ������ 1
                        textBox.Text = _settings.DnsServer1;
                        break;
                    case 5: // DNS ������ 2
                        textBox.Text = _settings.DnsServer2;
                        break;
                    case 6: // ����� ������
                        textBox.Text = _settings.LeaseTime.ToString();
                        break;
                    case 7: // ������� ����� �����
                        textBox.Text = _settings.HostnamePrefix;
                        break;
                }
                index++;
            }
        }
        
        // ���������� ������� ������ ����������
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // ����� ������ DHCP
                _settings.UseInternalDhcp = rbInternalDhcp.IsChecked ?? true;
                
                // ��������� �������� DHCP
                TextBox? externalDhcpTextBox = FindTextBoxInGroupBox(gbExternalSettings);
                if (externalDhcpTextBox != null)
                    _settings.ExternalDhcpServer = externalDhcpTextBox.Text;
                
                // ��������� ����������� DHCP - ���������
                ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
                if (networkComboBox != null && networkComboBox.SelectedItem != null)
                    _settings.NetworkInterface = (networkComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "(�������������)";
                
                // ��������� ��������� ���� � gbInternalSettings
                var textBoxes = FindTextBoxesInGroupBox(gbInternalSettings);
                int index = 0;
                foreach (var textBox in textBoxes)
                {
                    switch (index)
                    {
                        case 0: // ��������� IP �����
                            _settings.StartIpAddress = textBox.Text;
                            break;
                        case 1: // �������� IP �����
                            _settings.EndIpAddress = textBox.Text;
                            break;
                        case 2: // ����� �������
                            _settings.SubnetMask = textBox.Text;
                            break;
                        case 3: // ����
                            _settings.Gateway = textBox.Text;
                            break;
                        case 4: // DNS ������ 1
                            _settings.DnsServer1 = textBox.Text;
                            break;
                        case 5: // DNS ������ 2
                            _settings.DnsServer2 = textBox.Text;
                            break;
                        case 6: // ����� ������
                            if (int.TryParse(textBox.Text, out int leaseTime))
                                _settings.LeaseTime = leaseTime;
                            break;
                        case 7: // ������� ����� �����
                            _settings.HostnamePrefix = textBox.Text;
                            break;
                    }
                    index++;
                }
                
                // ������������ � JSON � ���������� � ����
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SETTINGS_FILE_PATH, json);
                
                MessageBox.Show("��������� DHCP ������� ���������", "����������", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"������ ���������� ��������: {ex.Message}", "������", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // ��������������� ������ ��� ������ ��������� ����������
        
        private StackPanel? FindStackPanelWithButtons()
        {
            // ������� StackPanel � �������������� �����������, ���������� ������
            if (this.Content is Grid mainGrid)
            {
                foreach (var child in mainGrid.Children)
                {
                    if (child is StackPanel mainStack)
                    {
                        foreach (var subChild in mainStack.Children)
                        {
                            if (subChild is StackPanel horizontalStack && 
                                horizontalStack.Orientation == Orientation.Horizontal)
                            {
                                bool hasButtons = false;
                                foreach (var item in horizontalStack.Children)
                                {
                                    if (item is Button)
                                    {
                                        hasButtons = true;
                                        break;
                                    }
                                }
                                
                                if (hasButtons)
                                    return horizontalStack;
                            }
                        }
                    }
                }
            }
            
            return null;
        }
        
        private TextBox? FindTextBoxInGroupBox(GroupBox groupBox)
        {
            if (groupBox == null) return null;
            
            if (groupBox.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is TextBox textBox)
                        return textBox;
                }
            }
            
            return null;
        }
        
        private ComboBox? FindComboBoxInGroupBox(GroupBox groupBox)
        {
            if (groupBox == null) return null;
            
            if (groupBox.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is ComboBox comboBox)
                        return comboBox;
                }
            }
            
            return null;
        }
        
        private List<TextBox> FindTextBoxesInGroupBox(GroupBox groupBox)
        {
            var textBoxes = new List<TextBox>();
            
            if (groupBox == null) return textBoxes;
            
            if (groupBox.Content is Grid grid)
            {
                foreach (var child in grid.Children)
                {
                    if (child is TextBox textBox)
                        textBoxes.Add(textBox);
                }
            }
            
            return textBoxes;
        }

        // ��������������� ����� ��� ��������� ���������� �������� ����������
        public NetworkInterface? GetSelectedNetworkInterface()
        {
            ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
            if (networkComboBox != null && networkComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is NetworkInterface iface)
            {
                return iface;
            }
            
            // ���� ������� "�������������" ��� ��������� �� ������, ���������� ������ �������� ���������
            return _availableInterfaces.FirstOrDefault();
        }
    }
}