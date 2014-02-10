using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;

namespace Timesheet.Model
{
    public static class Pagamento
    {
        public static double Horas { get; set; }
        public static int DiasTrabalhados { get; set; }

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

        public static string Salario()
        {
            return (Horas * Configuracao.ValorHr).ToString("#.##");
        }

        public static string SalarioEsperado()
        {
            return (Configuracao.HrsEsperadas * Configuracao.ValorHr).ToString("#.##");
        }

        public static string Media()
        {
            var media = (Configuracao.HrsEsperadas - Horas) / (QuantidadeDiasUteis() - DiasTrabalhados);

            if (media < 0)
                return "00:00";
            else if (media >= 24)
                return "Mais que 24h";

            var hrsDiarias = new DateTime(2014, 1, 1, 0, 0, 0).AddHours(media).ToShortTimeString();

            return hrsDiarias; 
        }

        public static void CarregarDadosTimesheet()
        {
            using (StreamReader sr = new StreamReader(Configuracao.Path))
            {
                var listHoras = new List<double>();
                var linha = sr.ReadLine();
                var linhaAnterior = linha;

                //Para sair do cabeçalho
                linha = sr.ReadLine();

                if (linha != null)
                    DiasTrabalhados = 1;

                while (linha != null)
                {
                    var dados = linha.Split(';');

                    if (!string.IsNullOrWhiteSpace(dados[3]) && dados.Length > 4)
                    {
                        var entrada = Convert.ToDateTime(dados[1]);
                        var saida = Convert.ToDateTime(dados[3]);

                        var totalHrs = (saida - entrada).TotalHours;
                        listHoras.Add(totalHrs);
                    }

                    linhaAnterior = linha;
                    linha = sr.ReadLine();

                    if (linha != null)
                        if (linhaAnterior.Split(';')[0].Trim() != linha.Split(';')[0].Trim())
                            DiasTrabalhados++;
                }

                Horas = Convert.ToInt32(listHoras.Sum());
                sr.Close();
            }
        }
    }
}
