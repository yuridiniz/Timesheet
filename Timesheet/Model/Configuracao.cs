using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public static class Configuracao
    {
        public static double HrsEsperadas { get; set; }
        public static int QtdFeriados { get; set; }
        public static double ValorHr { get; set; }

        public static bool ExibirPretencao { get; set; }
        public static bool ExibirValor { get; set; }

        public static int TempoInativo { get; set; }

        public static string Diretorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/";
        public static string Logs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Logs/";
        public static string PathConfig = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config";
        public static string Config = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config/Config.ini";
        public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Relatorio.txt";

        public static void CarregarConfiguracoes()
        {
            using (StreamReader sr = new StreamReader(Config))
            {
                var linha = sr.ReadLine();

                while (linha != null)
                {
                    try
                    {
                        var dados = linha.Split('=');
                        var prop = dados[0].Trim();
                        var valor = dados[1].Trim();

                        switch (prop)
                        {
                            case "HORA": HrsEsperadas = Convert.ToInt32(valor); break;
                            case "VALOR_HORA": ValorHr = Convert.ToInt32(valor); break;
                            case "QTD_FERIADOS": QtdFeriados = Convert.ToInt32(valor); break;
                            case "EXIBIR_PRETENCAO": ExibirPretencao = Convert.ToBoolean(valor); break;
                            case "EXIBIR_VALOR_ATUAL": ExibirValor = Convert.ToBoolean(valor); break;
                            case "TEMPO_INATIVO": TempoInativo = Convert.ToInt32(valor); break;
                            default: break;
                        }

                    }
                    catch(Exception e)
                    {
                        //Provavelmente uma linha em branco ou um comentário
                    }

                    linha = sr.ReadLine();
                }

                sr.Close();
            }
        }
    }
}
