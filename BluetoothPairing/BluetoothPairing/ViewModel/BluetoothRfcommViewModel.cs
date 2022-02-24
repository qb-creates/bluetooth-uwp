using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using GalaSoft.MvvmLight.Views;
using BluetoothPairing.Messaging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Rfcomm;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace BluetoothPairing.ViewModel
{
    public class BluetoothRfcommViewModel : ViewModelBase
    {
        private NavigationService navigationService;
        private BluetoothDevice bluetoothDevice;
        private StreamSocket stream;
        private DataWriter tx;
        private DataReader rx;
        private StringBuilder sb = new StringBuilder();
        private bool test = false;
        public BluetoothRfcommViewModel(NavigationService navigationService)
        {
            this.navigationService = navigationService;
            GoBackCommand = new RelayCommand(GoBack);
            Messenger.Default.Register<BluetoothRfcommMessage>(this, ReceiveMessage);
        }
        public RelayCommand GoBackCommand { get; private set; }
        public string RfcommData { get; set; }
        private void GoBack()
        {
            bluetoothDevice.ConnectionStatusChanged -= BluetoothDevice_ConnectionStatusChanged;
            test = true;
            bluetoothDevice?.Dispose();
            navigationService.NavigateTo("BluetoothListing");
        }
        public void ReceiveMessage(BluetoothRfcommMessage action)
        {
            test = false;
            ConnectDevice(action.DeviceInformation);
        }
        private async void ConnectDevice(DeviceInformation deviceInfo)
        {
            bluetoothDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id).AsTask();
            bluetoothDevice.ConnectionStatusChanged += BluetoothDevice_ConnectionStatusChanged;
            QueryServices();
        }
        private void BluetoothDevice_ConnectionStatusChanged(BluetoothDevice sender, object args)
        {
            Debug.WriteLine("Connection Status: " + bluetoothDevice.ConnectionStatus);
        }

        private async void QueryServices()
        {
            RfcommDeviceServicesResult result = await bluetoothDevice.GetRfcommServicesAsync();
            if(result.Services.Count > 0)
            {
                RfcommDeviceService service = result.Services[0];
                // Create a socket and connect to the target
                stream = new StreamSocket();

                await stream.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);
                rx = new DataReader(stream.InputStream);
                tx = new DataWriter(stream.OutputStream);
                ReadData();
            }
        }
        private async void ReadData()
        {
            while (!test)
            {
                await rx.LoadAsync(1);
                byte symbol = rx.ReadByte();
                if (symbol != 0x0D)
                    sb.Append((char)symbol);
                else
                {
                    Debug.WriteLine(sb.ToString().Replace("\n", ""));
                    RfcommData = sb.ToString().Replace("\n", "");
                    sb.Clear();
                    RaisePropertyChanged(nameof(RfcommData));
                }
            }
            stream.Dispose();
        }
    }
}
