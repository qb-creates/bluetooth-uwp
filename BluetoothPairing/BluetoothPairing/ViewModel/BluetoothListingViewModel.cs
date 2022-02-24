using GalaSoft.MvvmLight.Threading;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using Windows.Devices.Enumeration;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Views;
using BluetoothPairing.Services;
using GalaSoft.MvvmLight.Messaging;
using BluetoothPairing.Messaging;

namespace BluetoothPairing.ViewModel
{
    public class BluetoothListingViewModel : ViewModelBase
    {
        private NavigationService navigationService;
        private BluetoothService bluetoothService;
        private BluetoothViewModel selectedDevice;
        public BluetoothListingViewModel(NavigationService navigationService, BluetoothService bluetoothService)
        {
            this.navigationService = navigationService;
            this.bluetoothService = bluetoothService;
            StartListenerCommand = new RelayCommand(StartBluetoothListener);
            bluetoothService.OnListenerChanged += BluetoothService_OnListenerChanged;
        }
        public RelayCommand StartListenerCommand { get; private set; }
        public RelayCommand StartExperienceCommand { get; private set; }
        public ObservableCollection<BluetoothViewModel> KnownDevices => bluetoothService.KnownDevices;
        public bool ListenerIsActive { get; private set; }
        public bool StartExperienceEnabled => SelectedDevice == null ? false : SelectedDevice.IsPaired;
        public BluetoothViewModel SelectedDevice
        {
            get => selectedDevice;
            set
            {
                selectedDevice = value;
                RaisePropertyChanged(nameof(StartExperienceEnabled));
                RaisePropertyChanged(nameof(SelectedDevice));
            }
        }
        private void StartBluetoothListener()
        {
            bluetoothService.StartListener();
        }
        private void StopListener()
        {
            bluetoothService.StopListener();
        }
        private void BluetoothService_OnListenerChanged(bool isActive)
        {
            ListenerIsActive = isActive;
            RaisePropertyChanged(nameof(ListenerIsActive));
        }
    }
}
