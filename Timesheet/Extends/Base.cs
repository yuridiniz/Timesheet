using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Timesheet.Model;
using Timesheet;

namespace Controller.Extends
{
    public static class Base
    {

        public static void RegistrarEntrada(this Registro registro)
        {
            using (StreamWriter wr = new StreamWriter(Configuracao.Path, true))
            {
                wr.WriteLine("");
                wr.Write(registro.Dia + " ; ");
                wr.Write(registro.Entrada + " ; ");
                wr.Write(registro.Conferir + " ; ");
                wr.Close();
            }
        }

        public static void RegistrarSaida(this Registro registro)
        {
            using (StreamWriter wr = new StreamWriter(Configuracao.Path, true))
            {
                wr.Write(registro.Saida + " ; ");
                wr.Write(registro.Conferir + " ; ");
                wr.Write(string.IsNullOrEmpty(registro.Atividade) ? " " : registro.Atividade + "");

                wr.Close();
            }
        }
    }
}
