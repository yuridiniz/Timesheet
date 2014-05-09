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

        public static void RegistrarEntrada(this Registro registro, MainWindow contexto)
        {
            using (StreamWriter wr = new StreamWriter(Configuracao.Relatorio, true))
            {
                wr.WriteLine("");
                wr.Write(registro.Dia + " ; ");
                wr.Write(registro.Entrada + " ; ");
                wr.Write(registro.Conferir + " ; ");
                wr.Close();
            }

            contexto.notifyIcon1.BalloonTipTitle = "Timesheet";
            contexto.notifyIcon1.BalloonTipText = "Entrada Registrada: " + registro.Entrada;
            contexto.notifyIcon1.ShowBalloonTip(1000);
        }

        public static void RegistrarSaida(this Registro registro, MainWindow contexto)
        {
            using (StreamWriter wr = new StreamWriter(Configuracao.Relatorio, true))
            {
                wr.Write(registro.Saida + " ; ");
                wr.Write(registro.Conferir + " ; ");
                wr.Write(string.IsNullOrEmpty(registro.Atividade) ? " " : registro.Atividade + "");

                wr.Close();
            }


            contexto.notifyIcon1.BalloonTipTitle = "Timesheet";
            contexto.notifyIcon1.BalloonTipText = "Saída Registrada: " + registro.Saida;
            contexto.notifyIcon1.ShowBalloonTip(1000);
        }
    }
}
