using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TihiroRobotBeer.Model {
    public class SubTarefa {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public int Ordem { get; set; }
        public string Reles { get; set; }
        public int TempoInicial { get; set; }
        public int TempoExecucao { get; set; }
        [Indexed]
        public int Id_Tarefa { get; set; }
    }
}
