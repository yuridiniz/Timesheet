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
using Excel = Microsoft.Office.Interop.Excel;
using System.Threading;
using System.Diagnostics;
using System.Globalization;

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();

            RegistrarStartup();
            IniciarArquivos();
            
            Configuracao.CarregarConfiguracoes();
            Pagamento.CarregarDadosTimesheet();

            ExibirValores();

            btnEntrada.Click += btnEntrada_Click;
            btnSair.Click += btnSair_Click;
            btnExportar.Click += btnExportar_Click;
            btnConfig.Click += btnConfig_Click;

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
            if (File.Exists(Configuracao.Path))
            {
                try
                {
                    var ultimaLinha = UltimoRegistro();

                    if (!string.IsNullOrEmpty(ultimaLinha))
                    {
                        RegistrarFeriado(ultimaLinha);
                        AplicarEstadoBtn(ultimaLinha);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
            else
            {
                if (!Directory.Exists(Configuracao.Diretorio))
                    Directory.CreateDirectory(Configuracao.Diretorio);

                using (StreamWriter wr = new StreamWriter(Configuracao.Path, true))
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

            if (!Directory.Exists(Configuracao.PathConfig))
            {
                Directory.CreateDirectory(Configuracao.PathConfig);

                using (StreamWriter wr = new StreamWriter(Configuracao.Config, true))
                {
                    wr.WriteLine("# Dados para pagamento");
                    wr.WriteLine("");
                    wr.WriteLine("HORA = 176");
                    wr.WriteLine("VALOR_HORA = 30");
                    wr.WriteLine("QTD_FERIADOS = 0");
                    wr.WriteLine("");
                    wr.WriteLine("# Dados para exibição");
                    wr.WriteLine("");
                    wr.WriteLine("EXIBIR_PRETENCAO = true");
                    wr.WriteLine("EXIBIR_VALOR_ATUAL = true");

                    wr.Close();

                    MessageBox.Show("Arquivo de configuração criado em 'Meus Documento/Timesheet'");
                }
            }

        }

        public void ExibirValores()
        {
            if (!Configuracao.ExibirPretencao)
            {
                lblValorEsp.Visibility = Visibility.Hidden;
                lblValorEspTitulo.Visibility = Visibility.Hidden;
            }

            if (!Configuracao.ExibirValor)
            {
                lblValor.Visibility = Visibility.Hidden;
                lblValorTitulo.Visibility = Visibility.Hidden;
            }

            lblValor.Content = "R$ " + Pagamento.Salario();
            lblHrs.Content = Pagamento.Horas.ToString();
            lblValorEsp.Content = "R$ " + Pagamento.SalarioEsperado();
            lblMedia.Content = Pagamento.Media();
            lblHrsPretendidas.Content = Configuracao.HrsEsperadas.ToString();
            lblDiasUtes.Content = Pagamento.QuantidadeDiasUteis();
        }

       /// <summary>
       /// Varre o registro.txt e retorna ultimo dia cadastrado
       /// </summary>
       /// <returns>Ultimo dia cadastrado</returns>
        private string UltimoRegistro()
        {
            var linha = string.Empty;
            using (StreamReader sr = new StreamReader(Configuracao.Path))
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

            return linha;
        }

        /// <summary>
        /// Recebe o ultimo registro e grava registros em branco até o dia de hoje
        /// </summary>
        /// <param name="ultimoDia">Ultimo dia cadastrado (linha completa)</param>
        private void RegistrarFeriado(string ultimoDia)
        {
            var dia = ultimoDia.Split('/');
            if(dia.Length > 1)
            {
                var UltimoDiaRegistrado = Convert.ToInt32(dia[0]) + 1;
                var Hoje = DateTime.Now.Day;

                while (UltimoDiaRegistrado < Hoje)
                {
                    var registro = new Registro();

                    registro.Dia = DateTime.Parse(UltimoDiaRegistrado.ToString() + '/' + DateTime.Now.Month.ToString()).ToString("dd|MM|yyyy", CultureInfo.InvariantCulture) + ";";
                    registro.Entrada = ";";
                    registro.Conferir = ";";
                    registro.Saida = ";";
                    registro.Atividade = "";


                    registro.RegistrarEntrada();
                    registro.RegistrarSaida();

                    UltimoDiaRegistrado++;
                }
            }

        }

        /// <summary>
        /// Verifica qual foi a ultima ação salva e e habilita o botão para dar seguencia aos registros
        /// </summary>
        /// <param name="ultimaLinha"></param>
        public void AplicarEstadoBtn(string ultimaLinha)
        {
            var linhaStart = ultimaLinha.Length - 3;
            var entradaRegistada = ultimaLinha.Substring(linhaStart, ultimaLinha.Length - linhaStart).Contains(" ; ");
            if (entradaRegistada)
            {
                btnEntrada.IsEnabled = false;
                btnSair.IsEnabled = true;
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
                //Remove 4 minutos para bater com o timesheet do papel

                registro.Dia = DateTime.Now.ToString("dd/MM");
                registro.Entrada = DateTime.Now.AddMinutes(-4).ToShortTimeString();
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
        /// Evento de click para o botão exportar
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void btnExportar_Click(object s, EventArgs e)
        {
            try
            {
                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                    Process.GetProcessesByName("EXCEL")[i].Kill();

                //var desc = new EXCEL.Workbook();
                OpenFileDialog dialogo = new OpenFileDialog();
                dialogo.ShowDialog();

                var timesheetExcel = dialogo.FileName;

                if (string.IsNullOrEmpty(timesheetExcel))
                    return;

                var exApp = new Excel.Application();

                var work = exApp.Workbooks.Open(timesheetExcel, 0, false, 5, "", "", false, Excel.XlPlatform.xlWindows, "", true, false, 0, true, false, false);

                Excel.Sheets excelSheets = work.Worksheets;

                string currentSheet = "Yuri";
                Excel.Worksheet excelWorksheet = (Excel.Worksheet)excelSheets.get_Item(currentSheet);

                using (StreamReader sr = new StreamReader(Configuracao.Path))
                {
                    var linha = sr.ReadLine();
                    linha = sr.ReadLine();
                    int linhaEditavel = 1;
                    int linhaEditavelAnterior = linhaEditavel;
                    while (linha != null)
                    {
                        var dados = linha.Split(';');
                        if (!string.IsNullOrWhiteSpace(dados[3]) && dados.Length > 4)
                        {
                            linhaEditavel = ObterLinhaDaData(Convert.ToDateTime(dados[0]), excelWorksheet, linhaEditavel - 1);
                            if (linhaEditavel == linhaEditavelAnterior)
                                linhaEditavel++;

                            var entrada = Convert.ToDateTime(dados[1]);
                            var saida = Convert.ToDateTime(dados[3]);
                            var desc = dados[5];

                            var totalHrs = (saida - entrada).TotalHours;

                            Excel.Range cellEntrada = (Excel.Range)excelWorksheet.get_Range("D" + linhaEditavel, "D" + linhaEditavel);
                            Excel.Range cellSaida = (Excel.Range)excelWorksheet.get_Range("E" + linhaEditavel, "E" + linhaEditavel);
                            Excel.Range cellDesc = cellDesc = (Excel.Range)excelWorksheet.get_Range("H" + linhaEditavel, "H" + linhaEditavel);

                            cellEntrada.Value = entrada.ToShortTimeString();
                            cellSaida.Value = saida.ToShortTimeString();
                            cellDesc.Value = desc;

                            linhaEditavelAnterior = linhaEditavel;

                        }

                        linha = sr.ReadLine();
                    }

                    work.Save();
                    work.Close();
                    sr.Close();
                }

                MessageBox.Show("Exportado!");

                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                {
                    Process.GetProcessesByName("EXCEL")[i].Kill();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante o processo de exportação!");

            }

        }

        private void btnConfig_Click(object s, EventArgs e)
        {
            var slowTask = Task.Run<string>(() => teste());
            System.Diagnostics.Process.Start(Configuracao.Diretorio);
        }

        public int ObterLinhaDaData(DateTime data, Excel.Worksheet excelWorksheet, int startIndex)
        {
            var dataLinha = new DateTime();

            while(data.ToShortDateString() != dataLinha.ToShortDateString())
            {
                startIndex++;

                Excel.Range cellRotulo = (Excel.Range)excelWorksheet.get_Range("A" + startIndex, "A" + startIndex);

                if (cellRotulo.Value != null)
                {
                    var valor = Convert.ToString(cellRotulo.Value);
                    var dataLinha2 = Convert.ToDateTime(dataLinha.ToShortDateString());
                    var parse = DateTime.TryParse(valor, out dataLinha2);
                    if (parse == true)
                        dataLinha = dataLinha2;
                }

                if (startIndex == 100)
                    return -1;

            }

            return startIndex;
        }

        public string teste()
        {
            Thread.Sleep(8000);
            return "outra Thread";
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
