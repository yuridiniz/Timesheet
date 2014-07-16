using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;
using Timesheet.ModelContext;
using Timesheet.Repositorio;

namespace Timesheet.Model
{
    public static class Pagamento
    {
        public static double Horas
        {
            get
            {
                RegistroRepositorio db = new RegistroRepositorio();
                var listaRegistros = db.ListarRegistros();

                return listaRegistros.Sum(p => p.TotalHoras);
            }
        }

        public static double Hoje
        {
            get
            {
                RegistroRepositorio db = new RegistroRepositorio();
                var listaRegistros = db.ListarRegistros().Where(p => p.Dia == DateTime.Now.ToShortDateString() && p.StatusUsuario != Registro.Usuario.Working ).ToList();

                return listaRegistros.Sum(p => p.TotalHoras);
            }
        }
        public static int DiasTrabalhados { get; set; }
        public static int DiasRestantes { get; set; }

        public static int QuantidadeDiasUteis()
        {
            var qtdDias = 0;
            var dataAtual = DateTime.Now;
            dataAtual = dataAtual.AddDays((dataAtual.Day - 1) * -1);
            var mesAtual = dataAtual.Month;

            while(dataAtual.Month == mesAtual)
            {
                if (dataAtual.DayOfWeek != DayOfWeek.Sunday &&
                    dataAtual.DayOfWeek != DayOfWeek.Saturday)
                    qtdDias++;

                dataAtual = dataAtual.AddDays(1);
            }

            return qtdDias - Configuracao.QtdFeriados;
        }

        public static double Salario()
        {
            return Horas * Configuracao.ValorHr;
        }

        public static double SalarioEsperado()
        {
            return Configuracao.HrsEsperadas * Configuracao.ValorHr;
        }

        public static string Media()
        {
            var media = (double)(Configuracao.HrsEsperadas - Horas) / (DiasRestantes);

            if (media < 0)
                return "00:00";
            else if (media >= 24)
                return "Mais que 24h";

            var hrsDiarias = new DateTime(2014, 1, 1, 0, 0, 0).AddHours(media).ToShortTimeString();

            return hrsDiarias; 
        }

        public static void CarregarDadosTimesheet()
        {
            RegistroRepositorio db = new RegistroRepositorio();
            var listaRegistros = db.ListarRegistros();

            var hoje = DateTime.Now.AddDays(1);

            while (hoje.Month != DateTime.Now.AddMonths(1).Month)
            {
                if (hoje.DayOfWeek != DayOfWeek.Sunday
                    && hoje.DayOfWeek != DayOfWeek.Saturday)
                    DiasRestantes++;

                hoje = hoje.AddDays(1);
            }

            DiasTrabalhados = listaRegistros.Where(p => p.StatusUsuario != Registro.Usuario.Feriado
                                                   && p.DiaDaSemana != Registro.Semana.Domingo
                                                   && p.DiaDaSemana != Registro.Semana.Sabado)
                                                   .GroupBy(p => p.Dia).Count();
            db.Dispose();
        }
    }
}
