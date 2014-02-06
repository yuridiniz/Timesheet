using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Timesheet.Model;
using Controller.Extends;
using System.IO;
using Microsoft.Win32;
using System.Reflection;

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static string Diretorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RelatorioCatraca/";
        public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/RelatorioCatraca/Relatorio.txt";

        public MainWindow()
        {
            InitializeComponent();

            RegistrarStartup();
            IniciarArquivos();

            btnEntrada.Click += btnEntrada_Click;
            btnSair.Click += btnSair_Click;

        }

        /// <summary>
        /// Registra a aplicação para iniciar junto com o windows
        /// </summary>
        private void RegistrarStartup()
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);
                Assembly curAssembly = Assembly.GetExecutingAssembly();
                key.SetValue(curAssembly.GetName().Name, curAssembly.Location);
            }
            catch (Exception e)
            {
                MessageBox.Show("Ocorreu um erro ao tentar instalar o aplicativo na inicialização do Windows\n\n" + e.Message);
            }
        }

        /// <summary>
        /// Verifica a existencia dos arquivos, caso não exista ele cria
        /// </summary>
        private void IniciarArquivos()
        {
            if (File.Exists(Path))
            {
                try
                {
                    var linha = string.Empty;

                    using (StreamReader sr = new StreamReader(Path))
                    {
                        linha = sr.ReadLine();
                        var linhaAux = string.Empty;

                        var hasLinha = true;

                        while (hasLinha)
                        {
                            linhaAux = sr.ReadLine();

                            if (linhaAux == null)
                                hasLinha = false;
                            else
                                linha = linhaAux;
                        }

                        sr.Close();
                    }

                    var UltimoDiaRegistrado = Convert.ToInt32(linha.Split('/')[0]) + 1;
                    var Hoje = Convert.ToInt32(DateTime.Now.Day);

                    while (UltimoDiaRegistrado < Hoje)
                    {
                        var registro = new Registro();
                        //Remove 3 minutos para bater com o timesheet do papel

                        registro.Dia = UltimoDiaRegistrado.ToString() + ";";
                        registro.Entrada = ";";
                        registro.Conferir = ";";
                        registro.Saida = ";";
                        registro.Atividade = "";


                        registro.RegistrarEntrada();
                        registro.RegistrarSaida();

                        UltimoDiaRegistrado++;
                    }


                    var linhaStart = linha.Length - 3;
                    var entradaRegistada = linha.Substring(linhaStart, linha.Length - linhaStart).Contains(" ; ");
                    if (entradaRegistada)
                    {
                        btnEntrada.IsEnabled = false;
                        btnSair.IsEnabled = true;
                    }

                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (!Directory.Exists(Diretorio))
                    Directory.CreateDirectory(Diretorio);
                    
                using (StreamWriter wr = new StreamWriter(MainWindow.Path, true))
                {
                    wr.Write("Dia;");
                    wr.Write("Entrada;");
                    wr.Write("Status;");
                    wr.Write("Saida;");
                    wr.Write("Status;");
                    wr.Write("Atividade");
                    wr.Close();
                }
            }
        }

        /// <summary>
        /// Evento de click para o botão Entrar
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void btnEntrada_Click(object s, EventArgs e)
        {
            try
            {
                var registro = new Registro();
                //Remove 3 minutos para bater com o timesheet do papel

                registro.Dia = DateTime.Now.ToString("dd/MM");
                registro.Entrada = DateTime.Now.AddMinutes(-3).ToLongTimeString();
                registro.Conferir = (ckbConferir.IsChecked == true ? "Conferir" : "OK");

                registro.RegistrarEntrada();

                btnEntrada.IsEnabled = false;
                btnSair.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        /// <summary>
        /// Evento de click para o botão sair
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void btnSair_Click(object s, EventArgs e)
        {
            var desc = new DescricaoAtividades(this);
            desc.Show();
        }
    }
}
