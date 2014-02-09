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
        public static double HrsEsperadas { get; set; }
        public static double QtdDiasNoMes { get; set; }
        public static double ValorHr { get; set; }

        public static string Salario()
        {
            DadosDoPagamento();
            return Convert.ToString(Horas * ValorHr);
        }

        public static string SalarioEsperado()
        {
            DadosDoPagamento();
            return Convert.ToString(HrsEsperadas * ValorHr);
        }

        public static string Media()
        {
            DadosDoPagamento();
            var media = (HrsEsperadas - Horas) / (QtdDiasNoMes - DiasTrabalhados);
            return new DateTime(2014, 1, 1, 0, 0, 0).AddHours(media).ToShortTimeString();
        }

        private static void DadosDoPagamento()
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

                    if (dados.Length > 4)
                    {
                        var entrada = Convert.ToDateTime(dados[1]);
                        var saida = Convert.ToDateTime(dados[3]);

                        var totalHrs = (saida - entrada).TotalHours;
                        listHoras.Add(totalHrs);
                    }

                    linhaAnterior = linha;
                    linha = sr.ReadLine();

                    if (linha != null)
                    {
                        if (linhaAnterior.Split(';')[0].Trim() != linha.Split(';')[0].Trim())
                            DiasTrabalhados++;
                    }
                }

                Horas = listHoras.Sum();

                sr.Close();
            }

            //Ler as configurações do usuário
            using (StreamReader sr = new StreamReader(Configuracao.Config))
            {
                var linha = sr.ReadLine();
                var dados = linha.Split('=');
                HrsEsperadas = Convert.ToInt32(dados[1].Trim());

                linha = sr.ReadLine();
                dados = linha.Split('=');
                ValorHr = Convert.ToInt32(dados[1].Trim());

                linha = sr.ReadLine();
                dados = linha.Split('=');
                QtdDiasNoMes = Convert.ToInt32(dados[1].Trim());

                sr.Close();
            }
        }

    }
}
