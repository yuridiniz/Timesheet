using System;
using System.Collections.Generic;
using System.IO;
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

        //public static void RegistrarEntrada(MainWindow contexto, string status = null)
        //{
        //    var registro = new Registro();
        //    registro.Conferir = status == null ? "OK" : status;
        //    registro.Conferir = status == null ? "OK" : status;

        //    registro.SalvarEntrada(contexto, status);
        //}

        //private void SalvarEntrada(MainWindow contexto, string status)
        //{
        //    using (StreamWriter wr = new StreamWriter(Configuracao.Path, true))
        //    {
        //        wr.WriteLine("");
        //        wr.Write(this.Dia + " ; ");
        //        wr.Write(this.Entrada + " ; ");
        //        wr.Write(this.Conferir + " ; ");
        //        wr.Close();
        //    }

        //    contexto.notifyIcon1.BalloonTipTitle = "Timesheet";
        //    contexto.notifyIcon1.BalloonTipText = "Entrada Registrada: " + registro.Entrada;
        //    contexto.notifyIcon1.ShowBalloonTip(5000);

        //}

        //public Registro RegistrarSaida(this Registro registro, MainWindow contexto)
        //{
        //    using (StreamWriter wr = new StreamWriter(Configuracao.Path, true))
        //    {
        //        wr.Write(registro.Saida + " ; ");
        //        wr.Write(registro.Conferir + " ; ");
        //        wr.Write(registro.Atividade + "");

        //        wr.Close();
        //    }


        //    contexto.notifyIcon1.BalloonTipTitle = "Timesheet";
        //    contexto.notifyIcon1.BalloonTipText = "Saída Registrada: " + registro.Saida;
        //    contexto.notifyIcon1.ShowBalloonTip(5000);

        //    return this;
        //}
    }
}
