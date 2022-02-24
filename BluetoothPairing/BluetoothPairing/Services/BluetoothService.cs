using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using BluetoothPairing.ViewModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;

namespace BluetoothPairing.Services
{
    public class BluetoothService
    {
        // Additional properties we would like to have about the device.  https://msdn.microsoft.com/en-us/library/windows/desktop/ff521659(v=vs.85).aspx
        private readonly string[] requestedProperties = { "System.Devices.Aep.DeviceAddress", "System.Devices.Aep.IsConnected" };

        // Advanced Query Service to Find all Bluetooth and BluetoothLE Devices
        private const string aqsAllBluetoothLEDevices = "(System.Devices.Aep.ProtocolId:=\"{bb7bb05e-5972-42b5-94fc-76eaa7084d49}\" " +
                                                      "OR System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\")";
        private DeviceWatcher deviceWatcher;
        private List<DeviceInformation> UnknownDevices = new List<DeviceInformation>();
        public BluetoothService()
        {
            deviceWatcher = DeviceInformation.CreateWatcher(aqsAllBluetoothLEDevices, requestedProperties, DeviceInformationKind.AssociationEndpoint);
            deviceWatcher.Added += DeviceWatcher_Added;
            deviceWatcher.Updated += DeviceWatcher_Updated;
            deviceWatcher.Removed += DeviceWatcher_Removed;
            deviceWatcher.Stopped += DeviceWatcher_Stopped;
            deviceWatcher.EnumerationCompleted += DeviceWatcher_EnumerationCompleted;
        }
        public event Action<bool> OnListenerChanged;
        public ObservableCollection<BluetoothViewModel> KnownDevices { get; private set; } = new ObservableCollection<BluetoothViewModel>();
        public bool ListenerIsActive { get; private set; }
        public void StartListener()
        {
            Debug.WriteLine(deviceWatcher.Status);
            if (deviceWatcher.Status == DeviceWatcherStatus.Stopped || deviceWatcher.Status == DeviceWatcherStatus.Created || deviceWatcher.Status == DeviceWatcherStatus.Aborted)
            {
                KnownDevices.Clear();
                deviceWatcher.Start();
                ListenerIsActive = true;
                OnListenerChanged?.Invoke(true);
            }
            else if (deviceWatcher.Status == DeviceWatcherStatus.EnumerationCompleted || deviceWatcher.Status == DeviceWatcherStatus.Stopped)
            {
                deviceWatcher.Stop();
            }
        }

        public void StopListener()
        {
            if (deviceWatcher != null)
            {
                deviceWatcher.Added -= DeviceWatcher_Added;
                deviceWatcher.Updated -= DeviceWatcher_Updated;
                deviceWatcher.Removed -= DeviceWatcher_Removed;
                deviceWatcher.Stopped -= DeviceWatcher_Stopped;
                deviceWatcher.EnumerationCompleted -= DeviceWatcher_EnumerationCompleted;
                deviceWatcher.Stop();
                deviceWatcher = null;
                ListenerIsActive = false;
                OnListenerChanged?.Invoke(false);
            }
        }
        private BluetoothViewModel FindKnownDevice(string id)
        {
            foreach (BluetoothViewModel bleDeviceDisplay in KnownDevices)
            {
                if (bleDeviceDisplay.Id == id)
                {
                    return bleDeviceDisplay;
                }
            }
            return null;
        }
        private DeviceInformation FindUnknownDevice(string id)
        {
            foreach (DeviceInformation bleDeviceInfo in UnknownDevices)
            {
                if (bleDeviceInfo.Id == id)
                {
                    return bleDeviceInfo;
                }
            }
            return null;
        }
        private void DeviceWatcher_Added(DeviceWatcher sender, DeviceInformation deviceInfo)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                lock (this)
                {
                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Make sure the added device isn't already present in the list.
                        if (FindKnownDevice(deviceInfo.Id) == null)
                        {
                            if (deviceInfo.Name != string.Empty)
                            {
                                KnownDevices.Add(new BluetoothViewModel(deviceInfo));
                            }
                            else
                            {
                                UnknownDevices.Add(deviceInfo);
                            }
                        }
                    }
                    Debug.WriteLine(String.Format("Added {0}{1}", deviceInfo.Id, deviceInfo.Name));
                }
            });
        }
        private void DeviceWatcher_Updated(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                lock (this)
                {
                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        BluetoothViewModel bleDeviceDisplay = FindKnownDevice(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            // Device is already being displayed - update UX.
                            bleDeviceDisplay.Update(deviceInfoUpdate);
                            return;
                        }

                        DeviceInformation deviceInfo = FindUnknownDevice(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            deviceInfo.Update(deviceInfoUpdate);

                            // If device has been updated with a name, add it to our known device list and display it on the UI.
                            if (deviceInfo.Name != String.Empty)
                            {
                                KnownDevices.Add(new BluetoothViewModel(deviceInfo));
                                UnknownDevices.Remove(deviceInfo);
                            }
                        }
                    }
                 //   Debug.WriteLine(String.Format("Updated {0}{1}", deviceInfoUpdate.Id, ""));
                }
            });
        }
        private void DeviceWatcher_Removed(DeviceWatcher sender, DeviceInformationUpdate deviceInfoUpdate)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                lock (this)
                {
                    // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                    if (sender == deviceWatcher)
                    {
                        // Find the corresponding DeviceInformation in the collection and remove it.
                        BluetoothViewModel bleDeviceDisplay = FindKnownDevice(deviceInfoUpdate.Id);
                        if (bleDeviceDisplay != null)
                        {
                            KnownDevices.Remove(bleDeviceDisplay);
                        }

                        DeviceInformation deviceInfo = FindUnknownDevice(deviceInfoUpdate.Id);
                        if (deviceInfo != null)
                        {
                            UnknownDevices.Remove(deviceInfo);
                        }
                    }
                   // Debug.WriteLine(String.Format("Removed {0}{1}", deviceInfoUpdate.Id, ""));
                }
            });
        }
        private void DeviceWatcher_EnumerationCompleted(DeviceWatcher sender, object args)
        {
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
            {
                ListenerIsActive = false;
                OnListenerChanged?.Invoke(false);
            });
            Debug.WriteLine("Enumeration Completed");
        }
        private void DeviceWatcher_Stopped(DeviceWatcher sender, object args)
        {
            lock (this)
            {
                // Protect against race condition if the task runs after the app stopped the deviceWatcher.
                if (sender == deviceWatcher)
                {
                    DispatcherHelper.CheckBeginInvokeOnUI(StartListener);
                }
                Debug.WriteLine("Watcher Stopped");
            }
        }
    }
}
