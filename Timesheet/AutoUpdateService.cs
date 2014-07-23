﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Windows;
using Timesheet.Model;

namespace Timesheet
{
    public class AutoUpdateService
    {
        private static WebClient wc;
        private static string Json = "https://raw.githubusercontent.com/yuridiniz/Timesheet/master/Release/release.json";
        private static string Exe = "https://raw.githubusercontent.com/yuridiniz/Timesheet/master/Release/Timesheet.exe";
        private static string Update = "https://raw.githubusercontent.com/yuridiniz/Timesheet/master/Release/Update.exe";

        public static void Start()
        {
            wc = new WebClient();
            Random random = new Random();
            string url = Json + "?random=" + random.Next().ToString();
            string urlUpdate = Update + "?random=" + random.Next().ToString();
            

            if (!File.Exists(Configuracao.Diretorio + "Update.exe"))
            {
                wc.DownloadDataAsync(new Uri(urlUpdate));
                wc.DownloadDataCompleted += BaixarAtualizador;

            }
            else
            {
                
                wc.DownloadStringAsync(new Uri(url));
                wc.DownloadStringCompleted += VerificarVersao;
            }

        }

        private static void BaixarAtualizador(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                Random random = new Random();
                string url = Json + "?random=" + random.Next().ToString();

                File.WriteAllBytes(Configuracao.Diretorio + "Update.exe", e.Result);
                wc.DownloadStringAsync(new Uri(url));
                wc.DownloadStringCompleted += VerificarVersao;
            }
            catch (Exception)
            {

            }
        }

        private static void BaixarVersao()
        {
            Random random = new Random();
            string url = Exe + "?random=" + random.Next().ToString();

            wc.DownloadDataAsync(new Uri(url));
            wc.DownloadDataCompleted += DownloadConcluido;
        }

        private static void VerificarVersao(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string jsonData = e.Result;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<Update> novaVersao = serializer.Deserialize<List<Update>>(jsonData);

                if (novaVersao.First().Versao != Configuracao.Versao && ExibirDados(novaVersao))
                    BaixarVersao();

            }
            catch (Exception)
            {
                
            }
        }
        
        private static void DownloadConcluido(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                File.WriteAllBytes(Configuracao.Diretorio + "Timeshee.exe", e.Result);
                Process.Start(Configuracao.Diretorio + "Update.exe", Environment.CurrentDirectory);
            }
            catch (Exception)
            {
                
            }
            
        }

        private static bool ExibirDados(List<Update> versoes)
        {
            StringBuilder str = new StringBuilder();
            str.AppendLine(string.Format("Nova verão desenvolvida: {0} \nVersão atual: {1} \n", versoes.First().Versao, Configuracao.Versao));

            foreach (var versao in versoes)
            {
                if (Convert.ToDecimal(versao.Versao) > Convert.ToDecimal(Configuracao.Versao))
                {
                    str.AppendLine("\nVersao: " + versao.Versao);
                    foreach (var descricao in versao.Descricao)
                        str.AppendLine("- " + descricao);
                }
                else
                    break;
            }

            var resultado = MessageBox.Show(str.ToString(), "Nova versão desenvolvida", MessageBoxButton.YesNo, MessageBoxImage.Question);

            return resultado == MessageBoxResult.Yes;
        }

    }
}
