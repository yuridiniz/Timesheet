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
using System.Data;
using Forms = System.Windows.Forms;
using System.Drawing;
using Timesheet.ModelContext;

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool entrada = false;
        System.Timers.Timer a;
        public Forms.NotifyIcon notifyIcon1;
        bool Notificando = false;

        public MainWindow()
        {
            InitializeComponent();

            notifyIcon1 = new Forms.NotifyIcon();
            notifyIcon1.Icon = new Icon(SystemIcons.Information, 40, 40);
            notifyIcon1.Visible = true;
            notifyIcon1.Text = "Timesheet";

            RegistrarStartup();
            IniciarArquivos();
            
            Configuracao.CarregarConfiguracoes();
            Pagamento.CarregarDadosTimesheet();

            ExibirValores();

            //Contador
            a = new System.Timers.Timer();
            a.Interval = 1000;
            a.Elapsed += Cronometro;
            a.Start();

            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            btnEntrada.Click += btnEntrada_Click;
            btnSair.Click += btnSair_Click;
            btnExportar.Click += btnExportar_Click;
            notifyIcon1.DoubleClick += notifyIcon1_DoubleClick;
            btnConfig.Click += btnConfig_Click;
            this.StateChanged += MainWindow_StateChanged;

            btnRegistrarAtv.Click += (e, s) => { new CadastrarAtividade().ShowDialog(); };
            btnClose.Click += (e, s) => { this.WindowState = System.Windows.WindowState.Minimized; };
            bar.MouseDown += (e, s) => { this.DragMove(); };

        }

        #region Eventos

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

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            this.Show();
            this.ShowInTaskbar = true;
            WindowState = WindowState.Normal;
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (WindowState == WindowState.Minimized)
            {
                this.ShowInTaskbar = false;
                this.Hide();

                notifyIcon1.BalloonTipTitle = "Timesheet";
                notifyIcon1.BalloonTipText = "Working in background";
                notifyIcon1.ShowBalloonTip(1000);
            }
        }

        /// <summary>
        /// Evento disparado a cada segundo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Cronometro(object sender, System.Timers.ElapsedEventArgs e)
        {
            var hrsElapsed = Inactivity.GetLastInputTime();
            var ultimoRegistro = UltimoRegistro().Split(';');
            bool VerificaEntradaRegistrada = ultimoRegistro.Length <= 4;
            if (!Notificando)
            {
                string data = DateTime.Now.AddSeconds(-1 * hrsElapsed).ToString();

                if (VerificaEntradaRegistrada)
                {
                    var entrada = DateTime.Parse(ultimoRegistro[0].Trim() + "/" + DateTime.Now.Year + " " + ultimoRegistro[1] + ":00");
                    var diferenca = DateTime.Now - entrada;
                    var p = entrada.AddSeconds(diferenca.TotalSeconds);

                    Dispatcher.Invoke(new Action(() =>
                    {
                        int hr = Convert.ToInt32(this.lblHrs.Content);
                        this.lblHrs.Content = (int)(Pagamento.Horas + diferenca.TotalSeconds / (60 * 60));
                        this.lblValor.Content = string.Format("{0:C}", (Convert.ToInt32(Pagamento.Salario()) + Configuracao.ValorHr * (diferenca.TotalSeconds / (60 * 60))));
                    }));


                    if (hrsElapsed > Configuracao.TempoInativo * 60)
                    {
                        Notificando = true;
                        Dispatcher.Invoke(new Action(() =>
                        {
                            this.Hide();
                            this.Activate();
                            this.Topmost = true;  // important
                            this.Topmost = false; // important
                            this.Focus();         // important

                        }));

                        var resultado = MessageBox.Show("O Sistema ficou inativo desde " + data + " deseja registrar como uma saída?", "logout detectado", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        Dispatcher.Invoke(new Action(() =>
                        {
                            this.Show();

                        }));

                        if (resultado == MessageBoxResult.Yes)
                        {
                            var registro = new Registro();
                            //Adiciona 3 minutus para bater com o timesheet de papel

                            registro.Saida = DateTime.Parse(data).AddMinutes(3).ToShortTimeString();
                            registro.Atividade = " ";
                            registro.Conferir = "OK";

                            registro.RegistrarSaida(this);


                            Thread.Sleep(4000);

                            registro = new Registro();
                            //Remove 4 minutos para bater com o timesheet do papel

                            registro.Dia = DateTime.Now.ToString("dd/MM");
                            registro.Entrada = DateTime.Now.AddMinutes(-4).ToShortTimeString();
                            registro.Conferir = "OK";

                            registro.RegistrarEntrada(this);

                        }
                        else
                        {
                            if (DateTime.Now >= DateTime.Parse(DateTime.Parse(data).ToShortDateString() + " 23:59:59"))
                            {
                                a.Elapsed -= Cronometro;

                                var registro = new Registro();
                                //Adiciona 3 minutus para bater com o timesheet de papel

                                registro.Saida = DateTime.Now.ToShortTimeString();
                                registro.Conferir = "OK";
                                registro.Atividade = " ";

                                registro.RegistrarSaida(this);

                                Thread.Sleep(1200);

                                registro = new Registro();
                                //Remove 4 minutos para bater com o timesheet do papel

                                registro.Dia = DateTime.Now.ToString("dd/MM");
                                registro.Entrada = DateTime.Now.ToShortTimeString();
                                registro.Conferir = "OK";

                                registro.RegistrarEntrada(this);

                                a.Elapsed += Cronometro;
                            }

                        }

                        Notificando = false;
                    }

                    if (DateTime.Now >= DateTime.Parse("23:59:59"))
                    {
                        a.Elapsed -= Cronometro;

                        var registro = new Registro();
                        //Adiciona 3 minutus para bater com o timesheet de papel

                        registro.Saida = DateTime.Now.ToShortTimeString();
                        registro.Conferir = "OK";
                        registro.Atividade = " ";

                        registro.RegistrarSaida(this);

                        Thread.Sleep(1200);

                        registro = new Registro();
                        //Remove 4 minutos para bater com o timesheet do papel

                        registro.Dia = DateTime.Now.ToString("dd/MM");
                        registro.Entrada = DateTime.Now.ToShortTimeString();
                        registro.Conferir = "OK";

                        registro.RegistrarEntrada(this);

                        a.Elapsed += Cronometro;

                    }
                }
            }
        }


        /// <summary>
        /// Captura o travamento da sessão do windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!entrada)
            {
                string[] linhas = new string[] { DateTime.Now.ToString()};
                System.IO.File.WriteAllLines(Configuracao.Logs + "SwUser.log", linhas);
                entrada = true;
                a.Elapsed -= Cronometro;
            }
            else
            {
                a.Elapsed += Cronometro;
                if (System.IO.File.Exists(Configuracao.Logs + "SwUser.log"))
                {
                    this.Hide();
                    this.Activate();
                    this.Topmost = true;  // important
                    this.Topmost = false; // important
                    this.Focus();         // important

                    var linha = System.IO.File.ReadAllLines(Configuracao.Logs + "SwUser.log");
                    var data = DateTime.Now.AddMinutes(-4).ToShortTimeString();
                    var resultado = MessageBox.Show("Foi registrado um logout as " + linha[0] + " deseja registrar como uma saída?", "logout detectado", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    this.Show();

                    if (resultado == MessageBoxResult.Yes)
                    {
                        var registro = new Registro();
                        //Adiciona 3 minutus para bater com o timesheet de papel

                        registro.Saida = DateTime.Parse(linha[0]).AddMinutes(3).ToShortTimeString();
                        registro.Atividade = " ";
                        registro.Conferir = "OK";

                        registro.RegistrarSaida(this);

                        Thread.Sleep(3000);

                        registro = new Registro();
                        //Remove 4 minutos para bater com o timesheet do papel

                        registro.Dia = DateTime.Now.ToString("dd/MM");
                        registro.Entrada = data;
                        registro.Conferir = "OK";

                        registro.RegistrarEntrada(this);

                    }

                    System.IO.File.Delete(Configuracao.Logs + "SwUser.log");
                }

                entrada = false;
            }
        }

        /// <summary>
        /// Método que grava o momento do shutdown do windows
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            string[] linhas = new string[] { DateTime.Now.ToString() };
            System.IO.File.WriteAllLines(Configuracao.Logs + "ShutUser.log", linhas);
            entrada = true;
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

                registro.RegistrarEntrada(this);

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
            //DataTable dt = new DataTable(Configuracao.Path);

            //var exApp = new Excel.Application();
            //exApp.Visible = true;

            //Excel.Workbook wb = exApp.Workbooks.Add(Excel.XlWBATemplate.xlWBATWorksheet);
            //Excel.Worksheet ws = (Excel.Worksheet)wb.Worksheets[1];

            Task.Run(() => ExportarExcel());
        }

        /// <summary>
        /// Abre a pasta de configuração
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void btnConfig_Click(object s, EventArgs e)
        {
            System.Diagnostics.Process.Start(Configuracao.Diretorio);
        }

        #endregion

        #region Excel

        /// <summary>
        /// Exporta o excel
        /// </summary>
        private void ExportarExcel()
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
                exApp.Visible = true;

                Excel.Worksheet excelWorksheet = null;
                foreach (Excel.Worksheet worksheet in work.Worksheets)
                    excelWorksheet = worksheet;

                if (excelWorksheet == null)
                    return;

                using (StreamReader sr = new StreamReader(Configuracao.Relatorio))
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
                            Thread.Sleep(2500);
                            linhaEditavel = ObterLinhaDaData(Convert.ToDateTime(dados[0]), excelWorksheet);
                            if (linhaEditavel == linhaEditavelAnterior)
                                linhaEditavel++;
                            else if (linhaEditavel < linhaEditavelAnterior)
                            {
                                var diferenca = linhaEditavelAnterior - linhaEditavel;
                                linhaEditavel += (diferenca + 1);
                            }

                            if (linhaEditavel != -1)
                            {
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

                                //cellEntrada.Value = "";
                                //cellSaida.Value = "";
                                //cellDesc.Value = "";

                                linhaEditavelAnterior = linhaEditavel;
                            }

                        }

                        linha = sr.ReadLine();
                    }

                    work.Save();
                    work.Close();
                    sr.Close();
                }

                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                {
                    Process.GetProcessesByName("EXCEL")[i].Kill();
                }

                Process.Start(timesheetExcel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante o processo de exportação!");

            }
        }

        /// <summary>
        /// Obtem a linha da data para edição
        /// </summary>
        /// <param name="data"></param>
        /// <param name="excelWorksheet"></param>
        /// <param name="startIndex"></param>
        /// <returns></returns>
        public int ObterLinhaDaData(DateTime data, Excel.Worksheet excelWorksheet)
        {
            var dataLinha = new DateTime();
            int startIndex = 0;
            while (data.ToShortDateString() != dataLinha.ToShortDateString())
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

        #endregion

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
            if (System.IO.File.Exists(Configuracao.Logs + "ShutUser.log"))
            {
                this.Hide();
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important

                var linha = System.IO.File.ReadAllLines(Configuracao.Logs + "ShutUser.log");
                var data = DateTime.Now.AddMinutes(-4).ToShortTimeString();

                var resultado = MessageBox.Show("O sistema foi desligado as " + linha[0] + " deseja registrar como uma saída?", "shutdown detectado", MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {
                    var registro = new Registro();
                    //Adiciona 3 minutus para bater com o timesheet de papel

                    registro.Saida = DateTime.Parse(linha[0]).AddMinutes(3).ToShortTimeString();
                    registro.Atividade = " ";
                    registro.Conferir = "OK";

                    registro.RegistrarSaida(this);

                    registro = new Registro();
                    //Remove 4 minutos para bater com o timesheet do papel

                    registro.Dia = DateTime.Now.ToString("dd/MM");
                    registro.Entrada = data;
                    registro.Conferir = "OK";

                    registro.RegistrarEntrada(this);
                }

                System.IO.File.Delete(Configuracao.Logs + "ShutUser.log");
            }
            else if (System.IO.File.Exists(Configuracao.Logs + "SwUser.log"))
            {
                this.Hide();
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important

                var linha = System.IO.File.ReadAllLines(Configuracao.Logs + "SwUser.log");
                var data = DateTime.Now.AddMinutes(-4).ToShortTimeString();
                var resultado = MessageBox.Show("Foi registrado um logout as " + linha[0] + " deseja registrar como uma saída?", "logout detectado", MessageBoxButton.YesNo, MessageBoxImage.Question);

                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {
                    var registro = new Registro();
                    //Adiciona 3 minutus para bater com o timesheet de papel

                    registro.Saida = DateTime.Parse(linha[0]).AddMinutes(3).ToShortTimeString();
                    registro.Atividade = " ";
                    registro.Conferir = "OK";

                    registro.RegistrarSaida(this);

                    registro = new Registro();
                    //Remove 4 minutos para bater com o timesheet do papel

                    registro.Dia = DateTime.Now.ToString("dd/MM");
                    registro.Entrada = data;
                    registro.Conferir = "OK";

                    registro.RegistrarEntrada(this);

                }

                System.IO.File.Delete(Configuracao.Logs + "SwUser.log");
            }
            else
            {
                var ultimaLinha = UltimoRegistro();
                if (ultimaLinha.Contains("Dia;Entrada;Status;Saida;Status"))
                {
                    this.Hide();
                    this.Activate();
                    this.Topmost = true;  // important
                    this.Topmost = false; // important
                    this.Focus();         // important

                    var data = DateTime.Now.AddMinutes(-4).ToShortTimeString();
                    var resultado = MessageBox.Show("Registrar entrada?", "Iniciando mês", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    this.Show();

                    if (resultado == MessageBoxResult.Yes)
                    {
                        var registro = new Registro();
                        //Remove 4 minutos para bater com o timesheet do papel

                        registro.Dia = DateTime.Now.ToString("dd/MM");
                        registro.Entrada = data;
                        registro.Conferir = "OK";

                        registro.RegistrarEntrada(this);

                    }
                }

            }

            if (!Directory.Exists(Configuracao.Logs))
                Directory.CreateDirectory(Configuracao.Logs);

            if (!File.Exists(Configuracao.Atividades))
                File.Create(Configuracao.Atividades);

            if (File.Exists(Configuracao.Relatorio))
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

                using (StreamWriter wr = new StreamWriter(Configuracao.Relatorio, true))
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
                    wr.WriteLine("");
                    wr.WriteLine("# Dados de registro");
                    wr.WriteLine("");
                    wr.WriteLine("# tempo em minutos para que seja registrado uma saída quando o sistema estiver inativo");
                    wr.WriteLine("TEMPO_INATIVO = 20");

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

            lblValor.Content = string.Format("{0:C}",Pagamento.Salario());
            lblHrs.Content = string.Format("{0:0.00}",Pagamento.Horas);
            lblValorEsp.Content = string.Format("{0:C}", Pagamento.SalarioEsperado());
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
            using (StreamReader sr = new StreamReader(Configuracao.Relatorio))
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

                    registro.Dia = DateTime.Parse(UltimoDiaRegistrado.ToString() + '/' + DateTime.Now.Month.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture) + ";";
                    registro.Entrada = ";";
                    registro.Conferir = ";";
                    registro.Saida = ";";
                    registro.Atividade = " ";


                    registro.RegistrarEntrada(this);
                    registro.RegistrarSaida(this);

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

        




    }
}
