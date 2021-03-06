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

namespace Timesheet.Service
{
    /// <summary>
    /// Serviço de autoupdate do timesheet
    /// </summary>
    public class AutoUpdateService : BaseService
    {
        private WebClient wc;
        private const string DOMINIO = "https://bitbucket.org/yuridiniz/timesheet/raw/master/";
        private string Json = DOMINIO + "Release/release.json";
        private string Exe = DOMINIO + "Release/Timesheet.exe";
        private string Update = DOMINIO + "Release/Update.exe";

        public AutoUpdateService()
            : base(1)
        {

        }

        protected override void Acao(object sender, System.Timers.ElapsedEventArgs e)
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
                base.Pause();
                wc.DownloadStringAsync(new Uri(url));
                wc.DownloadStringCompleted += VerificarVersao;
            }
        }

        private void BaixarAtualizador(object sender, DownloadDataCompletedEventArgs e)
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

        private void BaixarVersao()
        {
            Random random = new Random();
            string url = Exe + "?random=" + random.Next().ToString();

            wc.DownloadDataAsync(new Uri(url));
            wc.DownloadDataCompleted -= BaixarAtualizador;
            wc.DownloadDataCompleted += DownloadConcluido;
        }

        private void VerificarVersao(object sender, DownloadStringCompletedEventArgs e)
        {
            try
            {
                string jsonData = e.Result;

                JavaScriptSerializer serializer = new JavaScriptSerializer();
                List<Update> novaVersao = serializer.Deserialize<List<Update>>(jsonData);

                if (novaVersao.First().Versao != Configuracao.Versao)
                {
                    ExibirDados(novaVersao);
                    BaixarVersao();
                }
                else
                    base.Resume();

            }
            catch (Exception)
            {
                base.Resume();
            }


        }
        
        private void DownloadConcluido(object sender, DownloadDataCompletedEventArgs e)
        {
            try
            {
                File.WriteAllBytes(Configuracao.Diretorio + "TimesheetNew.exe", e.Result);
                var parametro = "--pathData:" + Environment.CurrentDirectory.ToString() + "|" + Configuracao.Diretorio;
                Process.Start(Configuracao.Diretorio + "Update.exe", parametro);
                Process.GetCurrentProcess().Kill();
            }
            catch (Exception)
            {
                
            }
            
        }

        private void ExibirDados(List<Update> versoes)
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

            MessageBox.Show(str.ToString(), "Nova versão desenvolvida", MessageBoxButton.OK, MessageBoxImage.Question);
        }

    }
}
