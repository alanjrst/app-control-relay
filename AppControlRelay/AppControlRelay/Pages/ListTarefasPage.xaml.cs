using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TihiroRobotBeer.Model;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace TihiroRobotBeer.Pages
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class ListTarefasPage : ContentPage {
        public ObservableCollection<Tarefa> listTarefa { get; set; }
        public ListTarefasPage()
		{
			InitializeComponent ();

            Title = "Beer - Tarefas";

            var database = new DataBaseTihiroBeer<Tarefa>();            
            listTarefa = new ObservableCollection<Tarefa>(database.BuscarTodos<Tarefa>());
            listViewTarefas.ItemsSource = listTarefa;
        }

        public async void NovaTarefa() {
            await Navigation.PushAsync(new TarefaPage(new Tarefa()));
        }

        public async void OnDelete(object sender, EventArgs e) {
            var mi = ((MenuItem)sender);
            var idTarefa = Int16.Parse(mi.CommandParameter.ToString());
            var dataBase = new DataBaseTihiroBeer<Tarefa>();
            dataBase.Excluir<Tarefa>(idTarefa);
            var subTarefaRemover = listTarefa.Where(s => s.Id == idTarefa).FirstOrDefault();
            if (subTarefaRemover != null) {
                listTarefa.Remove(subTarefaRemover);
            }
        }

        private void ItemSelectedDevice(object sender, SelectedItemChangedEventArgs e) {
            if (listViewTarefas.SelectedItem == null) {
                return;
            }
            Tarefa tarefa = listViewTarefas.SelectedItem as Tarefa;
            ((ListView)sender).SelectedItem = null;
            TarefaPage tarefapage = new TarefaPage(tarefa);
            Navigation.PushAsync(tarefapage);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            var database = new DataBaseTihiroBeer<Tarefa>();
            listTarefa = new ObservableCollection<Tarefa>(database.BuscarTodos<Tarefa>());
            listViewTarefas.ItemsSource = listTarefa;

        }
    }
}