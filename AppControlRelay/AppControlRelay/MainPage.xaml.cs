using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TihiroRobotBeer {
    public partial class MainPage : ContentPage {

        IBluetoothLE blueLE;
        IAdapter adapter;
        ObservableCollection<IDevice> deviceList;
        IDevice device;

        public MainPage() {
            InitializeComponent();

            Title = "Beer - Dispositivos";

            blueLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceList = new ObservableCollection<IDevice>();
            lv.ItemsSource = deviceList;
        }

        private async void OnItemClicked(object sender , EventArgs e) {
            var state = blueLE.State;
            if(state == BluetoothState.On) {
                deviceList.Clear();
                
                adapter.DeviceDiscovered += (s , a) => 
                {
                    //if(a.Device.Name != null && a.Device.Name.ToString().Contains("ZL"))
                        deviceList.Add(a.Device);
                };
                await adapter.StartScanningForDevicesAsync();
            }
        }

        private void ItemSelectedDevice(object sender , SelectedItemChangedEventArgs e) {
            if(lv.SelectedItem == null) {
                return;
            }
            device = lv.SelectedItem as IDevice;
            ((ListView)sender).SelectedItem = null;
            DeviceControlPage deviceControl = new DeviceControlPage(device);
            Navigation.PushAsync(deviceControl);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
        }
    }
}
