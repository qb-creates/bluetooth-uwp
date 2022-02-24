using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Devices.Enumeration;
using Windows.UI.Xaml.Media.Imaging;

namespace BluetoothPairing.ViewModel
{
    /// <summary>
    ///     Display class used to represent a BluetoothLEDevice in the Device list
    /// </summary>
    public class BluetoothViewModel : ViewModelBase
    {
        private bool deviceSelected;
        public BluetoothViewModel(DeviceInformation deviceInfoIn)
        {
            RemoveDeviceCommand = new RelayCommand(RemoveDevice);
            DeviceInformation = deviceInfoIn;
            GetConnectionStatus();
            UpdateGlyphBitmapImage();
        }
        public RelayCommand RemoveDeviceCommand { get; }
        public DeviceInformation DeviceInformation { get; private set; }
        public string Id => DeviceInformation.Id;
        public string Name => DeviceInformation.Name;
        public bool IsPaired => DeviceInformation.Pairing.IsPaired;
        public bool IsConnected => (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true;
        public BitmapImage GlyphBitmapImage { get; private set; }
        public string ConnectionStatus { get; private set; }
        public bool DeviceSelected
        {
            get
            {
                return deviceSelected;
            }
            set
            {
                deviceSelected = value;
                RaisePropertyChanged(nameof(DeviceSelected));
                if (deviceSelected)
                {
                    PairDevice();
                }
                Debug.WriteLine(DeviceInformation.Id + "     " + DeviceInformation.Name + "    " + DeviceInformation.Kind);
            }
        }
        public void Update(DeviceInformationUpdate deviceInfoUpdate)
        {
            DeviceInformation.Update(deviceInfoUpdate);
            GetConnectionStatus();
            RaisePropertyChanged(nameof(Id));
            RaisePropertyChanged(nameof(Name));
            RaisePropertyChanged(nameof(IsPaired));
            RaisePropertyChanged(nameof(IsConnected));
            RaisePropertyChanged(nameof(ConnectionStatus));
            UpdateGlyphBitmapImage();
        }
        private void GetConnectionStatus()
        {
            if (!DeviceInformation.Pairing.IsPaired)
            {
                ConnectionStatus = "Not Paired";
            }
            else if (DeviceInformation.Pairing.IsPaired && (bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == false)
            {
                ConnectionStatus = "Paired";
            }
            else if ((bool?)DeviceInformation.Properties["System.Devices.Aep.IsConnected"] == true)
            {
                ConnectionStatus = "Connected";
            }
        }
        private async void UpdateGlyphBitmapImage()
        {
            DeviceThumbnail deviceThumbnail = await DeviceInformation.GetGlyphThumbnailAsync();
            var glyphBitmapImage = new BitmapImage();
            await glyphBitmapImage.SetSourceAsync(deviceThumbnail);
            GlyphBitmapImage = glyphBitmapImage;
            RaisePropertyChanged("GlyphBitmapImage");
        }
        private async void PairDevice()
        {
            if (!DeviceInformation.Pairing.IsPaired)
            {
                DevicePairingResult pairResults = await DeviceInformation.Pairing.PairAsync();
            }
        }
        private async void RemoveDevice()
        {
            if (DeviceInformation.Pairing.IsPaired)
            {
                DeviceSelected = false;
                DeviceUnpairingResult pairResults = await DeviceInformation.Pairing.UnpairAsync();
            }
        }
    }
}
