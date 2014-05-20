using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public class Log
    {
        public string Data { get; set; }
        public string Analisado { get; set; }
        public static string Cabecalho {get {return "Dia;Entrada;StatusEntrada;Saida;StatusSaida";}}
    }
}
