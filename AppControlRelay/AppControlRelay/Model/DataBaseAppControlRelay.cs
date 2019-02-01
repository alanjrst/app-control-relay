using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace TihiroRobotBeer.Model {
    class DataBaseAppControlRelay<T> : IDisposable where T : class {

        public SQLiteConnection _conexao;

        public DataBaseAppControlRelay() {
            var config = DependencyService.Get<IConfig>();
            var path = DependencyService.Get<IConfig>().DiretorioDB;
            
            _conexao = new SQLiteConnection(System.IO.Path.Combine(path, "TihiroBeer.bd3"));            

            _conexao.CreateTable<Tarefa>();
            _conexao.CreateTable<SubTarefa>();
        }

        public void Novo(T tobject) {
            _conexao.Insert(tobject);
        }
        public void Novo(List<T> listObjects) {
            _conexao.InsertAll(listObjects);
        }

        public void Update(T tobject) {
            int teste = _conexao.Update(tobject);
        }

        public void Salvar(List<T> listObjects) {
            _conexao.Update(listObjects);
        }

        public int ExcluirTodos() {
            return _conexao.DeleteAll<T>();
        }

        public List<T> BuscarTodos<T>() where T : new() {
            List<T> list = new List<T>();
            var result = _conexao.Table<T>();
            foreach (var item in result) {
                list.Add(item);
            }
            return list;
        }

        public void Excluir<T>(int id) {
             _conexao.Delete<T>(id);
        }

        public List<SubTarefa> BuscarSubTarefasPorTarefaId(int id) {
            List<SubTarefa> listSubTarefa = new List<SubTarefa>();
            var subtarefas = from t in _conexao.Table<SubTarefa>() where t.Id_Tarefa == id select t;

            foreach (var item in subtarefas) {
                listSubTarefa.Add(item);
            }
            return listSubTarefa;
        }

        public int GetLastInsertId() {
            return (int)SQLite3.LastInsertRowid(_conexao.Handle);
        }

        public void Dispose() {
            _conexao.Close();
            _conexao.Dispose();
        }
    }
}
