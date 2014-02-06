using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public class Registro
    {
        public string Nome { get; set; }
        public string Dia { get; set; }
        public string Entrada { get; set; }
        public string Saida { get; set; }
        
        public string Atividade { get; set; }
        public string Conferir { get; set; }
        
    }
}
