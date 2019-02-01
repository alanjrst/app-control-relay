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
	public partial class TarefaPage : ContentPage
	{
        public ObservableCollection<SubTarefa> listSubTarefas = new ObservableCollection<SubTarefa>();
        public Tarefa tarefaEdit { get; set; }
        public TarefaPage (Tarefa tarefa)
		{
			InitializeComponent ();
            Title = "Nova Tarefa";

            if(tarefa.Id == 0) {
                listViewTarefas.ItemsSource = listSubTarefas;
            } else {
                Title = "Editar Tarefa";
                tarefaEdit = tarefa;
                Nome.Text = tarefa.Nome;
                Descricao.Text = tarefa.Descricao;
                TempoDuracao.Text = tarefa.TempoDuracao.ToString();
                TempoIntervalo.Text = tarefa.TempoIntervalo.ToString();
                var dataBase = new DataBaseTihiroBeer<SubTarefa>();
                listSubTarefas = new ObservableCollection<SubTarefa>(dataBase.BuscarSubTarefasPorTarefaId(tarefa.Id));
                listViewTarefas.ItemsSource = listSubTarefas;

                MenuSalvar.Text = "Confirmar";
            }

            
        }

        public async void AddSubTarefa() {
            if (MenuSalvar.Text == "Confirmar") {
                if (ReleTempoInicial.Text != "" && ReleTempoExecucao.Text != "" && ReleNumero.SelectedIndex != -1)
                {
                    SubTarefa subTarefa = new SubTarefa() {Id_Tarefa = tarefaEdit.Id , TempoInicial = Int16.Parse(ReleTempoInicial.Text), Reles = (string)ReleNumero.SelectedItem, Ordem = listSubTarefas.Count + 1 , TempoExecucao = Int16.Parse(ReleTempoExecucao.Text) };
                    var dataBase = new DataBaseTihiroBeer<SubTarefa>();
                    dataBase.Novo(subTarefa);
                    dataBase.Dispose();
                    listSubTarefas.Add(subTarefa);
                    ReleTempoInicial.Text = "";
                    ReleNumero.SelectedIndex = 0;
                    ReleTempoExecucao.Text = "";
                }
                else
                {
                    DisplayAlert("Nova Tarefa", "Informe o rele , tempo inicial e tempo duração.", "OK");
                }
            } else {
                if (ReleTempoInicial.Text != "" && ReleTempoExecucao.Text != "" && ReleNumero.SelectedIndex != -1) {
                    SubTarefa subTarefa = new SubTarefa() { TempoInicial = Int16.Parse(ReleTempoInicial.Text), TempoExecucao = Int16.Parse(ReleTempoExecucao.Text) , Reles = (string)ReleNumero.SelectedItem, Ordem = listSubTarefas.Count + 1 };
                    listSubTarefas.Add(subTarefa);
                    ReleTempoInicial.Text = "";
                    ReleNumero.SelectedIndex = 0;
                    ReleTempoExecucao.Text = "";
                }
                else
                {
                    DisplayAlert("Nova Tarefa", "Informe o rele , tempo inicial e tempo duração.", "OK");
                }
            }
                
        }
        private void ItemSelectedDevice(object sender, SelectedItemChangedEventArgs e)
        {
            if (listViewTarefas.SelectedItem == null)
            {
                return;
            }
            SubTarefa tarefa = listViewTarefas.SelectedItem as SubTarefa;
            EnteredName.Text = string.Empty;

            overlay.IsVisible = true;

            EnteredName.Focus();
        }

        void OnButtonClicked(object sender, EventArgs args)
        {
            EnteredName.Text = string.Empty;

            overlay.IsVisible = true;

            EnteredName.Focus();
        }

        void OnOKButtonClicked(object sender, EventArgs args)
        {
            overlay.IsVisible = false;

            DisplayAlert("Result", string.Format("You entered {0}", EnteredName.Text), "OK");
        }

        void OnCancelButtonClicked(object sender, EventArgs args)
        {
            overlay.IsVisible = false;
        }

        public async void OnDelete(object sender, EventArgs e) {
            var item = ((MenuItem)sender);
            var ordem = Int16.Parse(item.CommandParameter.ToString());            
            var subTarefaRemover = listSubTarefas.Where(s => s.Ordem == ordem).FirstOrDefault();
            if (subTarefaRemover != null)
            {
                listSubTarefas.Remove(subTarefaRemover);
            }

            if (MenuSalvar.Text == "Confirmar")
            {
                var dataBase = new DataBaseTihiroBeer<SubTarefa>();
                dataBase.Excluir<SubTarefa>(subTarefaRemover.Id);
                dataBase.Dispose();

                var dataBaseTarefa = new DataBaseTihiroBeer<Tarefa>();
                tarefaEdit.Nome = Nome.Text;
                tarefaEdit.Descricao = Descricao.Text;
                tarefaEdit.TempoDuracao = TempoDuracao.Text == "" ? 0 : int.Parse(TempoDuracao.Text);
                dataBaseTarefa.Update(tarefaEdit);
                dataBaseTarefa.Dispose();

            }
            Reordenar();
            //DisplayAlert("Delete Context Action", mi.CommandParameter + " delete context action", "OK");
        }

        private void Reordenar()
        {
            for (int i = 0; i < listSubTarefas.Count; i++)
            {
                listSubTarefas[i].Ordem = i + 1;
            }
        }

        public async void SalvarTarefas() {
            if(MenuSalvar.Text == "Salvar") {
                if (Nome.Text == "" && listSubTarefas.Count == 0) {
                    DisplayAlert("Tarefa", "Os campos devem serem preenchidos", "Cancelar");
                } else {
                    Tarefa tarefa = new Tarefa() {
                        Nome = Nome.Text,
                        Descricao = Descricao.Text,
                        TempoDuracao = TempoDuracao.Text == "" ? 0 : int.Parse(TempoDuracao.Text) ,
                        TempoIntervalo = TempoIntervalo.Text == "" ? 0 : int.Parse(TempoIntervalo.Text) , Status = ""
                    };

                    var dataBase = new DataBaseTihiroBeer<Tarefa>();
                    dataBase.Novo(tarefa);
                    tarefa.Id = dataBase.GetLastInsertId();
                    dataBase.Dispose();

                    var dataBaseSubTarefa = new DataBaseTihiroBeer<SubTarefa>();
                    foreach (var item in listSubTarefas) {
                        item.Id_Tarefa = tarefa.Id;
                        dataBaseSubTarefa.Novo(item);
                    }
                    dataBaseSubTarefa.Dispose();

                    Navigation.PopAsync();
                }
            } else {
                if (Nome.Text != "" && listSubTarefas.Count != 0) {
                    var dataBase = new DataBaseTihiroBeer<Tarefa>();
                    tarefaEdit.Nome = Nome.Text;
                    tarefaEdit.Descricao = Descricao.Text;
                    tarefaEdit.TempoDuracao = TempoDuracao.Text == "" ? 0 : int.Parse(TempoDuracao.Text);
                    dataBase.Update(tarefaEdit);
                    Navigation.PopAsync();
                } else {
                    DisplayAlert("Tarefa", "Os campos devem serem preenchidos", "Cancelar");
                }
            }
            
        }
    }
}