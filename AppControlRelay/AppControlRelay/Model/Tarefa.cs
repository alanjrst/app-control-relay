using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TihiroRobotBeer.Model {

    public class Tarefa {

        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string Nome { get; set; }
        public string Descricao { get; set; }
        public string Status { get; set; }
        public int TempoDuracao { get; set; }
        public int TempoIntervalo { get; set; }
        [Ignore]
        public List<SubTarefa> SubTarefa { get; set; }
    }
}
