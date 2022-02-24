/*
  In App.xaml:
  <Application.Resources>
      <vm:ViewModelLocatorTemplate xmlns:vm="clr-namespace:Chargily.ViewModel"
                                   x:Key="Locator" />
  </Application.Resources>
  
  In the View:
  DataContext="{Binding Source={StaticResource Locator}, Path=ViewModelName}"
*/

using CommonServiceLocator;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Ioc;
using GalaSoft.MvvmLight.Views;
using BluetoothPairing.Services;
using BluetoothPairing.View;
using System;

namespace BluetoothPairing.ViewModel
{
    /// <summary>
    /// This class contains static references to all the view models in the
    /// application and provides an entry point for the bindings.
    /// <para>
    /// See http://www.mvvmlight.net
    /// See https://docs.microsoft.com/en-us/archive/msdn-magazine/2013/february/mvvm-ioc-containers-and-mvvm for SimpleIoc Explaination.
    /// </para>
    /// </summary>
    public class ViewModelLocator
    {
        static ViewModelLocator()
        {
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);
            SimpleIoc.Default.Register<BluetoothListingViewModel>();
            SimpleIoc.Default.Register<BluetoothRfcommViewModel>();
            SetupNavigation();
            SetupBluetooth();
        }

        private static void SetupNavigation()
        {
            var navigationService = new NavigationService();
            navigationService.Configure("BluetoothListing", typeof(BluetoothListingView));
            navigationService.Configure("Rfcomm", typeof(BluetoothRfcommView));
            SimpleIoc.Default.Register<NavigationService>(() => navigationService);
        }
        private static void SetupBluetooth()
        {
            var bluetoothService = new BluetoothService();
            SimpleIoc.Default.Register<BluetoothService>(() => bluetoothService);
        }
        public BluetoothListingViewModel BluetoothListingViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BluetoothListingViewModel>();
            }
        }
        public BluetoothRfcommViewModel BluetoothRfcommViewModel
        {
            get
            {
                return ServiceLocator.Current.GetInstance<BluetoothRfcommViewModel>();
            }
        }
    }
}
