using System;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;

namespace NetworkBootManagerUI.Services
{
    public class DhcpService
    {
        private bool _isRunning;
        private IPAddress? _serverIp;
        private IPAddress? _startIp;
        private IPAddress? _endIp;
        private IPAddress? _subnetMask;
        private IPAddress? _gateway;
        private IPAddress[]? _dnsServers;
        private int _leaseTime;
        private string? _hostnamePrefix;
        private bool _isProxyMode;
        private IPAddress? _externalDhcpServer;
        private UdpClient? _udpClient;
        private CancellationTokenSource? _cancellationTokenSource;
        private Task? _serverTask;

        public event EventHandler<string>? LogMessage;
        public event EventHandler<bool>? StatusChanged;

        public bool IsRunning => _isRunning;

        public DhcpService()
        {
            // Инициализация без внешних библиотек
        }

        public async Task StartServer(IPAddress serverIp, IPAddress startIp, IPAddress endIp, 
            IPAddress subnetMask, IPAddress? gateway, IPAddress[]? dnsServers, 
            int leaseTime, string? hostnamePrefix, bool isProxyMode = false, IPAddress? externalDhcpServer = null)
        {
            if (_isRunning)
            {
                return;
            }

            try
            {
                _serverIp = serverIp;
                _startIp = startIp;
                _endIp = endIp;
                _subnetMask = subnetMask;
                _gateway = gateway;
                _dnsServers = dnsServers;
                _leaseTime = leaseTime;
                _hostnamePrefix = hostnamePrefix;
                _isProxyMode = isProxyMode;
                _externalDhcpServer = externalDhcpServer;

                _cancellationTokenSource = new CancellationTokenSource();
                _udpClient = new UdpClient(67);
                _udpClient.EnableBroadcast = true;

                // Запускаем сервер в отдельной задаче
                _serverTask = Task.Run(() => RunDhcpServer(_cancellationTokenSource.Token));
                
                // Ждем, пока сервер запустится
                await Task.Delay(100);
                
                _isRunning = true;
                StatusChanged?.Invoke(this, true);
                LogMessage?.Invoke(this, $"DHCP сервер запущен на {_serverIp} в режиме {(_isProxyMode ? "прокси" : "обычном")}");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка запуска DHCP сервера: {ex.Message}");
                throw;
            }
        }

        private async Task RunDhcpServer(CancellationToken cancellationToken)
        {
            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var result = await _udpClient!.ReceiveAsync(cancellationToken);
                    var clientEndPoint = result.RemoteEndPoint;
                    var data = result.Buffer;

                    LogMessage?.Invoke(this, $"Получен DHCP пакет от {clientEndPoint}");

                    if (_isProxyMode && _externalDhcpServer != null)
                    {
                        // Режим прокси - перенаправляем запрос на внешний DHCP сервер
                        await ForwardDhcpRequest(data, clientEndPoint, _externalDhcpServer);
                    }
                    else
                    {
                        // Обычный режим - обрабатываем запрос локально
                        await ProcessDhcpRequest(data, clientEndPoint);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                // Сервер остановлен
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка в DHCP сервере: {ex.Message}");
            }
        }

        private async Task ForwardDhcpRequest(byte[] data, IPEndPoint clientEndPoint, IPAddress externalServer)
        {
            try
            {
                using (var forwardClient = new UdpClient())
                {
                    // Отправляем запрос на внешний DHCP сервер
                    await forwardClient.SendAsync(data, data.Length, new IPEndPoint(externalServer, 67));
                    
                    // Получаем ответ от внешнего сервера
                    var response = await forwardClient.ReceiveAsync();
                    
                    // Отправляем ответ клиенту
                    await _udpClient!.SendAsync(response.Buffer, response.Buffer.Length, clientEndPoint);
                    
                    LogMessage?.Invoke(this, $"Запрос перенаправлен на {externalServer} и получен ответ");
                }
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка при перенаправлении запроса: {ex.Message}");
            }
        }

        private async Task ProcessDhcpRequest(byte[] data, IPEndPoint clientEndPoint)
        {
            try
            {
                // Здесь должна быть логика обработки DHCP запроса
                // Это упрощенная реализация, которая просто отправляет ответ
                
                // Создаем простой DHCP ответ
                var response = new byte[300]; // Упрощенный размер ответа
                
                // Заполняем заголовок DHCP ответа
                response[0] = 2; // DHCP Reply
                response[1] = 1; // Ethernet
                response[2] = 6; // Адрес клиента
                
                // Копируем MAC-адрес клиента из запроса
                Array.Copy(data, 28, response, 28, 6);
                
                // Устанавливаем IP-адрес клиента (в реальности нужно выбрать из пула)
                var clientIp = _startIp!.GetAddressBytes();
                Array.Copy(clientIp, 0, response, 16, 4);
                
                // Устанавливаем IP-адрес сервера
                var serverIp = _serverIp!.GetAddressBytes();
                Array.Copy(serverIp, 0, response, 20, 4);
                
                // Отправляем ответ клиенту
                await _udpClient!.SendAsync(response, response.Length, clientEndPoint);
                
                LogMessage?.Invoke(this, $"Отправлен DHCP ответ клиенту {clientEndPoint}");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка при обработке запроса: {ex.Message}");
            }
        }

        public async Task StopServer()
        {
            if (!_isRunning)
            {
                return;
            }

            try
            {
                _cancellationTokenSource?.Cancel();
                _udpClient?.Close();
                
                if (_serverTask != null)
                {
                    await _serverTask;
                }
                
                _isRunning = false;
                StatusChanged?.Invoke(this, false);
                LogMessage?.Invoke(this, "DHCP сервер остановлен");
            }
            catch (Exception ex)
            {
                LogMessage?.Invoke(this, $"Ошибка остановки DHCP сервера: {ex.Message}");
                throw;
            }
        }

        public List<NetworkInterface> GetNetworkInterfaces()
        {
            return NetworkInterface.GetAllNetworkInterfaces()
                .Where(ni => ni.OperationalStatus == OperationalStatus.Up 
                    && ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
                .ToList();
        }

        public IPAddress? GetInterfaceIpAddress(NetworkInterface networkInterface)
        {
            var ipProps = networkInterface.GetIPProperties();
            var ipv4 = ipProps.UnicastAddresses
                .FirstOrDefault(addr => addr.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            
            return ipv4?.Address;
        }
    }
} 