using Java.Lang;
using Plugin.BLE;
using Plugin.BLE.Abstractions.Contracts;
using Plugin.BLE.Abstractions.Exceptions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TihiroRobotBeer.Model;
using TihiroRobotBeer.Pages;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TihiroRobotBeer
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class DeviceControlPage : ContentPage
    {

        IBluetoothLE blueLE;
        IAdapter adapter;
        IDevice deviceBluetooth;
        bool connected = false;
        IService service;
        ICharacteristic characteristic;
        public bool status { get; set; }
        public bool statusExecucao { get; set; }
        ObservableCollection<Tarefa> listTarefa = new ObservableCollection<Tarefa>();
        Thread thread = new Thread();
        Tarefa tarefaSelecionada;

        public DeviceControlPage(IDevice device)
        {
            InitializeComponent();

            blueLE = CrossBluetoothLE.Current;
            adapter = CrossBluetoothLE.Current.Adapter;
            deviceBluetooth = device;
            this.Title = device.Name;
            Nome.Text = "Nome: " + device.Name;

            ConnectDevice();

            var dataBase = new DataBaseTihiroBeer<Tarefa>();
            listTarefa = new ObservableCollection<Tarefa>(dataBase.BuscarTodos<Tarefa>());
            listViewTarefas.ItemsSource = listTarefa;

            status = true;
            statusExecucao = false;
        }

        public void Stop()
        {
            status = false;
            statusExecucao = false;
            FecharRele1();
            FecharRele2();
            FecharRele3();
            FecharRele4();
            BtStop.Text = "Parado";
        }

        public async void Tarefas()
        {
            await Navigation.PushAsync(new ListTarefasPage());
        }

        private void ItemSelectedDevice(object sender, SelectedItemChangedEventArgs e)
        {
            if (listViewTarefas.SelectedItem == null)
            {
                return;
            }
            tarefaSelecionada = listViewTarefas.SelectedItem as Tarefa;
            listViewTarefas.SelectedItem = null;
            Processar(tarefaSelecionada);
        }

        public async void ConnectDevice()
        {
            try
            {
                await adapter.ConnectToDeviceAsync(deviceBluetooth);
                getServiceDevice();
                var status = deviceBluetooth.State.ToString();
                Status.Text = "Status: " + status == "Connected" ? "DESCONECTADO" : "CONECTADO";
                connected = true;
            }
            catch (DeviceConnectionException e)
            {
                Status.Text = e.Message;
                connected = false;
            }
        }

        public async void DisconnectDevice()
        {
            try
            {
                await adapter.DisconnectDeviceAsync(deviceBluetooth);
                getServiceDevice();
                var status = deviceBluetooth.State.ToString();
                Status.Text = "Status: " + status == "Connected" ? "DESCONECTADO" : "CONECTADO";
                connected = true;
            }
            catch (DeviceConnectionException e)
            {
                Status.Text = e.Message;
                connected = false;
            }
        }

        public async void getServiceDevice()
        {
            service = await deviceBluetooth.GetServiceAsync(Guid.Parse("0000ffe0-0000-1000-8000-00805f9b34fb"));
            if (service != null)
            {
                getCharacteristicAsync(service);
            }
        }

        public async void getCharacteristicAsync(IService service)
        {
            characteristic = await service.GetCharacteristicAsync(Guid.Parse("0000ffe1-0000-1000-8000-00805f9b34fb"));
        }

        public void Processar(Tarefa tarefa)
        {
            TarefaExecutandoNome.Text = tarefa.Nome + " - Executando";
            if (!statusExecucao)
            {
                BtStop.Text = "Parar";
                statusExecucao = true;
                Task.Run(() =>
                {
                    status = true;
                    var dataBase = new DataBaseTihiroBeer<SubTarefa>();
                    List<SubTarefa> listSubTarefa = dataBase.BuscarSubTarefasPorTarefaId(tarefa.Id);
                    dataBase.Dispose();


                    for (int i = 0; i < tarefa.TempoDuracao; i++)
                    {
                        Device.BeginInvokeOnMainThread(() => {
                            Time.Text = "Tempo:" + i.ToString();
                        });
                            
                        if (!status)
                        {
                            break;
                        }
                        List<SubTarefa> listSubTarefaSecond = listSubTarefa.Where(x => x.TempoInicial == i).ToList();
                        foreach (var item in listSubTarefaSecond)
                        {
                            switch (item.Reles)
                            {
                                case "1":
                                    AbrirRele1();
                                    Debug.WriteLine("Abriu rele 1");
                                    break;
                                case "2":
                                    AbrirRele2();
                                    Debug.WriteLine("Abriu rele 2");
                                    break;
                                case "3":
                                    AbrirRele3();
                                    Debug.WriteLine("Abriu rele 3");
                                    break;
                                case "4":
                                    AbrirRele4();
                                    Debug.WriteLine("Abriu rele 4");
                                    break;
                            }
                            processarRele(item);
                        }
                        Thread.Sleep(1000);
                    }
                    statusExecucao = false;
                    Device.BeginInvokeOnMainThread(() => {
                        TarefaExecutandoNome.Text = "";
                        Time.Text = "Tempo:";
                    });
                });
                
            }
        }

        public void processarRele(SubTarefa subtarefa)
        {
            SubTarefa subtarefaNova = new SubTarefa() { TempoExecucao = subtarefa.TempoExecucao, Reles = subtarefa.Reles };
            var tempo_exec = subtarefaNova.TempoExecucao * 1000;


            Device.StartTimer(TimeSpan.FromSeconds(subtarefaNova.TempoExecucao), () =>
            {
                if (status)
                    switch (subtarefa.Reles)
                    {
                        case "1":
                            FecharRele1();
                            Debug.WriteLine("Fechou rele 1");
                            break;
                        case "2":
                            FecharRele2();
                            Debug.WriteLine("Fechou rele 2");
                            break;
                        case "3":
                            FecharRele3();
                            Debug.WriteLine("Fechou rele 3");
                            break;
                        case "4":
                            FecharRele4();
                            Debug.WriteLine("Fechou rele 4");
                            break;
                    }
                
                return false;
            });
        }

        protected virtual void OnDisappearing()
        {
            OnBackButtonPressed();
        }

        protected override bool OnBackButtonPressed()
        {
            base.OnBackButtonPressed();
            DisconnectDevice();
            return true;

        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var dataBase = new DataBaseTihiroBeer<Tarefa>();
            listTarefa = new ObservableCollection<Tarefa>(dataBase.BuscarTodos<Tarefa>());
            listViewTarefas.ItemsSource = listTarefa;
            ConnectDevice();
        }

        #region comandos reles
        public async void AbrirRele1()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";


                byte[] data = { (byte)0xC5, (byte)0x04, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }

        public async void FecharRele1()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x06, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }


        public async void AbrirRele2()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x05, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }

        public async void FecharRele2()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x07, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }


        public async void AbrirRele3()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x31, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }

        public async void FecharRele3()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x32, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }


        public async void AbrirRele4()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x33, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }

        public async void FecharRele4()
        {
            if (deviceBluetooth.State == Plugin.BLE.Abstractions.DeviceState.Connected)
            {
                var password = "12345678";

                byte[] data = { (byte)0xC5, (byte)0x34, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0x99, (byte)0xAA };

                data[2] = (byte)password[0];
                data[3] = (byte)password[1];
                data[4] = (byte)password[2];
                data[5] = (byte)password[3];
                data[6] = (byte)password[4];
                data[7] = (byte)password[5];
                data[8] = (byte)password[6];
                data[9] = (byte)password[7];

                await characteristic.WriteAsync(data);
            }
        }

        #endregion
    }
}