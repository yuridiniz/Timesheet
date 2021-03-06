﻿using System;
using System.Collections.Generic;
using System.Globalization;
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

        public static string Versao = "0.7.33";
        public static string Diretorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/";
        public static string Logs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Logs/";
        public static string DiretorioBkp = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Bkp/";
        public static string Atividades = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Atividades.txt";
        public static string PathConfig = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config";
        public static string Config = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config/Config.ini";
        public static string Relatorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Relatorio.txt";
        public static string Shutdown = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Logs/ShutUser.log";
        public static string Logout = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Logs/SwUser.log";
        public static string RelatorioLogs = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Bkp/Log_saida.log";

        public static string ConfigFile = "# Dados para pagamento;;HORA = 176;VALOR_HORA = 30;QTD_FERIADOS = 0;;# Dados para exibição;;EXIBIR_PRETENCAO = true;EXIBIR_VALOR_ATUAL = true;;# Dados de registro;;# tempo em minutos para que seja registrado uma saída quando o sistema estiver inativo;TEMPO_INATIVO = 20";

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
