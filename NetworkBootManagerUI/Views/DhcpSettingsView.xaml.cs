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
            
            // Инициализация настроек
            _settings = new DhcpSettings();
            
            // Получение доступных сетевых интерфейсов
            _availableInterfaces = GetNetworkInterfaces();
            
            // Заполнение комбобокса сетевыми интерфейсами
            PopulateNetworkInterfaces();
            
            // Загрузка настроек при запуске
            LoadSettings();
            
            // Привязка обработчика события к кнопке сохранения
            // Находим кнопку "Сохранить настройки DHCP" в нижней StackPanel
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
                        if (content == "Сохранить настройки DHCP")
                            saveButton = button;
                        else if (content == "Запустить сервер")
                            startButton = button;
                        else if (content == "Остановить сервер")
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

            // Добавление обработчиков для радиокнопок
            rbInternalDhcp.Checked += RbDhcpMode_Checked;
            rbExternalDhcp.Checked += RbDhcpMode_Checked;
        }

        // Обработчик для переключения режимов DHCP
        private void RbDhcpMode_Checked(object sender, RoutedEventArgs e)
        {
            // Эти привязки уже настроены в XAML через IsEnabled="{Binding ElementName=rbInternalDhcp, Path=IsChecked}"
            // Но можно добавить дополнительную логику при необходимости
        }

        // Получение списка сетевых интерфейсов
        public List<NetworkInterface> GetNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up 
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();
        }
        
        // Получение IP-адреса интерфейса
        public IPAddress? GetInterfaceIpAddress(NetworkInterface networkInterface)
        {
            var ipProps = networkInterface.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses
                .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            
            return ipv4?.Address;
        }

        // Заполнение комбобокса сетевыми интерфейсами
        private void PopulateNetworkInterfaces()
        {
            ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
            if (networkComboBox == null) return;

            networkComboBox.Items.Clear();
            
            // Добавление автоматического выбора
            networkComboBox.Items.Add(new ComboBoxItem { Content = "(Автоматически)" });
            
            // Добавление всех доступных интерфейсов
            foreach (var iface in _availableInterfaces)
            {
                var ipAddress = GetInterfaceIpAddress(iface);
                string displayName = $"{iface.Name} - {ipAddress}";
                networkComboBox.Items.Add(new ComboBoxItem { Content = displayName, Tag = iface });
            }
            
            // Выбор первого элемента по умолчанию
            networkComboBox.SelectedIndex = 0;
        }

        // Обработчики для кнопок запуска и остановки сервера
        private void StartServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Логика запуска DHCP сервера
                // Здесь вы можете использовать _settings и _availableInterfaces
                
                MessageBox.Show("DHCP сервер запущен", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при запуске DHCP сервера: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private void StopServer_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Логика остановки DHCP сервера
                
                MessageBox.Show("DHCP сервер остановлен", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при остановке DHCP сервера: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Класс для хранения настроек DHCP
        public class DhcpSettings
        {
            // Общие настройки
            public bool UseInternalDhcp { get; set; } = true;
            
            // Настройки внешнего DHCP
            public string ExternalDhcpServer { get; set; } = "";
            
            // Настройки внутреннего DHCP
            public string NetworkInterface { get; set; } = "(Автоматически)";
            public string StartIpAddress { get; set; } = "192.168.1.100";
            public string EndIpAddress { get; set; } = "192.168.1.200";
            public string SubnetMask { get; set; } = "255.255.255.0";
            public string Gateway { get; set; } = "";
            public string DnsServer1 { get; set; } = "";
            public string DnsServer2 { get; set; } = "";
            public int LeaseTime { get; set; } = 120;
            public string HostnamePrefix { get; set; } = "";
        }
        
        // Загрузка настроек из файла
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
                        // Применение настроек к элементам управления
                        ApplySettingsToUI();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Применение загруженных настроек к интерфейсу
        private void ApplySettingsToUI()
        {
            // Режим работы DHCP
            rbInternalDhcp.IsChecked = _settings.UseInternalDhcp;
            rbExternalDhcp.IsChecked = !_settings.UseInternalDhcp;
            
            // Настройки внешнего DHCP
            TextBox? externalDhcpTextBox = FindTextBoxInGroupBox(gbExternalSettings);
            if (externalDhcpTextBox != null)
                externalDhcpTextBox.Text = _settings.ExternalDhcpServer;
            
            // Настройки внутреннего DHCP - интерфейс
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
            
            // Остальные текстовые поля в gbInternalSettings
            var textBoxes = FindTextBoxesInGroupBox(gbInternalSettings);
            int index = 0;
            foreach (var textBox in textBoxes)
            {
                switch (index)
                {
                    case 0: // Начальный IP адрес
                        textBox.Text = _settings.StartIpAddress;
                        break;
                    case 1: // Конечный IP адрес
                        textBox.Text = _settings.EndIpAddress;
                        break;
                    case 2: // Маска подсети
                        textBox.Text = _settings.SubnetMask;
                        break;
                    case 3: // Шлюз
                        textBox.Text = _settings.Gateway;
                        break;
                    case 4: // DNS Сервер 1
                        textBox.Text = _settings.DnsServer1;
                        break;
                    case 5: // DNS Сервер 2
                        textBox.Text = _settings.DnsServer2;
                        break;
                    case 6: // Время аренды
                        textBox.Text = _settings.LeaseTime.ToString();
                        break;
                    case 7: // Префикс имени хоста
                        textBox.Text = _settings.HostnamePrefix;
                        break;
                }
                index++;
            }
        }
        
        // Обработчик нажатия кнопки сохранения
        private void SaveSettings_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Режим работы DHCP
                _settings.UseInternalDhcp = rbInternalDhcp.IsChecked ?? true;
                
                // Настройки внешнего DHCP
                TextBox? externalDhcpTextBox = FindTextBoxInGroupBox(gbExternalSettings);
                if (externalDhcpTextBox != null)
                    _settings.ExternalDhcpServer = externalDhcpTextBox.Text;
                
                // Настройки внутреннего DHCP - интерфейс
                ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
                if (networkComboBox != null && networkComboBox.SelectedItem != null)
                    _settings.NetworkInterface = (networkComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "(Автоматически)";
                
                // Остальные текстовые поля в gbInternalSettings
                var textBoxes = FindTextBoxesInGroupBox(gbInternalSettings);
                int index = 0;
                foreach (var textBox in textBoxes)
                {
                    switch (index)
                    {
                        case 0: // Начальный IP адрес
                            _settings.StartIpAddress = textBox.Text;
                            break;
                        case 1: // Конечный IP адрес
                            _settings.EndIpAddress = textBox.Text;
                            break;
                        case 2: // Маска подсети
                            _settings.SubnetMask = textBox.Text;
                            break;
                        case 3: // Шлюз
                            _settings.Gateway = textBox.Text;
                            break;
                        case 4: // DNS Сервер 1
                            _settings.DnsServer1 = textBox.Text;
                            break;
                        case 5: // DNS Сервер 2
                            _settings.DnsServer2 = textBox.Text;
                            break;
                        case 6: // Время аренды
                            if (int.TryParse(textBox.Text, out int leaseTime))
                                _settings.LeaseTime = leaseTime;
                            break;
                        case 7: // Префикс имени хоста
                            _settings.HostnamePrefix = textBox.Text;
                            break;
                    }
                    index++;
                }
                
                // Сериализация в JSON и сохранение в файл
                string json = JsonSerializer.Serialize(_settings, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(SETTINGS_FILE_PATH, json);
                
                MessageBox.Show("Настройки DHCP успешно сохранены", "Информация", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения настроек: {ex.Message}", "Ошибка", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        // Вспомогательные методы для поиска элементов управления
        
        private StackPanel? FindStackPanelWithButtons()
        {
            // Находим StackPanel с горизонтальной ориентацией, содержащую кнопки
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

        // Вспомогательный метод для получения выбранного сетевого интерфейса
        public NetworkInterface? GetSelectedNetworkInterface()
        {
            ComboBox? networkComboBox = FindComboBoxInGroupBox(gbInternalSettings);
            if (networkComboBox != null && networkComboBox.SelectedItem is ComboBoxItem selectedItem && selectedItem.Tag is NetworkInterface iface)
            {
                return iface;
            }
            
            // Если выбрано "Автоматически" или интерфейс не найден, возвращаем первый активный интерфейс
            return _availableInterfaces.FirstOrDefault();
        }
    }
}