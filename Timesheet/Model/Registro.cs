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
        public string Id { get; set; }
        public string Dia { get; set; }
        public string Entrada { get; set; }
        public string Saida { get; set; }
        public string Atividade { get; set; }
        public string StatusEntrada { get; set; }
        public string StatusSaida { get; set; }
        public Usuario StatusUsuario
        {
            get
            {
                if(string.IsNullOrEmpty(Saida) && string.IsNullOrEmpty(Entrada))
                    return Usuario.Feriado;
                else if (string.IsNullOrEmpty(Saida))
                    return Usuario.Working;
                else
                    return Usuario.Off;
            }
        }
        public double TotalHoras
        {
            get
            {
                DateTime entrada = new DateTime();
                DateTime saida;

                if (StatusUsuario == Usuario.Feriado)
                    return 0;
                else if (StatusUsuario == Usuario.Working)
                {
                    entrada = Convert.ToDateTime(Entrada);
                    saida = DateTime.Now;
                }
                else
                {
                    entrada = Convert.ToDateTime(Entrada);
                    saida = Convert.ToDateTime(Saida);
                }

                return (saida - entrada).TotalHours;
            }
        }

        public Semana DiaDaSemana
        {
            get
            {
                var dia = DateTime.Parse(Dia);
                var week = dia.DayOfWeek;

                switch (week)
                {
                    case DayOfWeek.Sunday: return Semana.Domingo;
                    case DayOfWeek.Monday: return Semana.Segunda;
                    case DayOfWeek.Tuesday: return Semana.Terça;
                    case DayOfWeek.Wednesday: return Semana.Quarta;
                    case DayOfWeek.Thursday: return Semana.Quinta;
                    case DayOfWeek.Friday: return Semana.Sexta;
                    case DayOfWeek.Saturday: return Semana.Sabado;
                    default: return Semana.Domingo;
                }
            }
        }

        public string TextoSemana
        {
            get
            {
                var dia = DateTime.Parse(Dia);
                var week = dia.DayOfWeek;

                switch (week)
                {
                    case DayOfWeek.Sunday: return "domingo";
                    case DayOfWeek.Monday: return "segunda-feira";
                    case DayOfWeek.Tuesday: return "terça-feira";
                    case DayOfWeek.Wednesday: return "quarta-feira";
                    case DayOfWeek.Thursday: return "quinta-feira";
                    case DayOfWeek.Friday: return "sexta-feira";
                    case DayOfWeek.Saturday: return "sabado";
                    default: return "domingo";
                }
            }
        }

        public static string Cabecalho = "Dia;Entrada;StatusEntrada;Saida;StatusSaida";

        public static Registro Entrar(DateTime data, MainWindow ctx)
        {
            var registro = new Registro();
            registro.Dia = data.ToString("dd/MM/yyyy");
            registro.Entrada = data.AddMinutes(-4).ToShortTimeString();

            if (data.Hour == 00 && data.Minute >= 00 + 4)
                registro.Entrada = data.ToShortTimeString();

            registro.StatusEntrada = "OK";

            ctx.notifyIcon1.BalloonTipTitle = "Entrada registrada";
            ctx.notifyIcon1.BalloonTipText = registro.Entrada;
            ctx.notifyIcon1.ShowBalloonTip(3000);

            return registro;
        }

        public static Registro Sair(DateTime data, Registro registro, MainWindow ctx)
        {
            registro.Saida = data.AddMinutes(4).ToShortTimeString();

            if (data.Hour == 23 && data.Minute >= 59 - 4)
                registro.Saida = data.ToShortTimeString();
                
            registro.StatusSaida = "OK";

            ctx.notifyIcon1.BalloonTipTitle = "Saída registrada";
            ctx.notifyIcon1.BalloonTipText = registro.Saida;
            ctx.notifyIcon1.ShowBalloonTip(3000);

            return registro;
        }

        public enum Usuario
        {
            Working,
            Feriado,
            Off
        }

        public enum Semana
        {
            Domingo,
            Segunda,
            Terça,
            Quarta,
            Quinta,
            Sexta,
            Sabado
        }
    }
}
