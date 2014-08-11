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
using System.IO;
using Microsoft.Win32;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using System.Data;
using Forms = System.Windows.Forms;
using System.Drawing;
using Timesheet.ModelContext;
using Timesheet.Repositorio;
using Timesheet.Service;

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public bool entrada = false;
        public System.Timers.Timer temporizador;
        public Forms.NotifyIcon IconeNotificacao;
        public bool Notificando = false;

        public MainWindow()
        {
            InitializeComponent();
            AdministrarProcessosTimesheet();

            //Instancias
            IconeNotificacao = new Forms.NotifyIcon();

            //Eventos
            btnClose.Click += (e, s) => { this.WindowState = System.Windows.WindowState.Minimized; };
            bar.MouseDown += (e, s) => { this.DragMove(); };
            btnConfig.Click += (e, s) => { System.Diagnostics.Process.Start(Configuracao.Diretorio); };
            btnExportar.Click += (e, s) => { Task.Run(() => Excel.ExportarExcel()); };
            btnRegistrarAtv.Click += (e, s) => { new CadastrarAtividade().ShowDialog(); };
            btnExportarTeste.Click += (e, s) => { Task.Run(() => Excel.CriarExcel()); };
            IconeNotificacao.Click += IconeNotificacao_Click;
            SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;
            SystemEvents.SessionEnding += SystemEvents_SessionEnding;
            this.StateChanged += MainWindow_StateChanged;

            //Propriedades
            IconeNotificacao.Icon = new Icon(Application.GetResourceStream(new Uri("pack://application:,,,/Images/clock-icon.ico")).Stream);

            //Serviços
            new AutoUpdateService();
            new AtividadeService();
            new LogSaidaService();

            //Esse timer é para ser um serviço com acesso a VIEW
            temporizador = new System.Timers.Timer();
            temporizador.Interval = 1000;
            temporizador.Elapsed += Cronometro;
            temporizador.Start();

        }

        /// <summary>
        /// Visualiza se existe outros processos do timesheet, caso exista finaliza essa e mantem a antiga, caso não exista
        /// inicia os processos
        /// </summary>
        private void AdministrarProcessosTimesheet()
        {
            if (Process.GetProcessesByName("Timesheet").Count() >= 2)
            {
                EventosPorProcesso.ExibirProcesso(Process.GetProcessesByName("Timesheet")[1].MainWindowHandle);
                this.Close();
            }
            else
                IniciaProcesso(0);
        }

        private void IniciaProcesso(int tentativa)
        {
            try
            {
                RegistrarStartup();
                IniciarArquivos();

                Configuracao.CarregarConfiguracoes();
                Pagamento.CarregarDadosTimesheet();

                VerificarSaida();
                ExibirValores();

            }
            catch (IOException e)
            {
                MessageBox.Show("O sistema sofreu uma falha, porfavor inicie novamente!");
                this.Close();
            }
            catch (Exception e)
            {
                if (tentativa > 5)
                    MessageBox.Show(e.Message);
                else
                    IniciaProcesso(++tentativa);
            }
        }

        private void IconeNotificacao_Click(object sender, EventArgs e)
        {
            var evento = (System.Windows.Forms.MouseEventArgs)e;
            if (evento.Button == Forms.MouseButtons.Left)
                AbrirJanela();
            else
                ExibirTooltipDeDados();
        }

        private void ExibirTooltipDeDados()
        {
            var db = new RegistroRepositorio();
            var ultimoRegistro = db.ObterUltimoRegistro();

            if (ultimoRegistro != null)
            {
                var entrada = DateTime.Parse(ultimoRegistro.Dia + " " + ultimoRegistro.Entrada);
                var diferenca = DateTime.Now - entrada;
                var hoje = new DateTime().AddHours(Pagamento.Hoje) + diferenca;

                var Horas = new DateTime().AddHours(Pagamento.Horas);
                IconeNotificacao.BalloonTipTitle = "Dados";
                IconeNotificacao.BalloonTipText = String.Format("Total:\t\t{0} \nHoje:\t\t{1}\nPagamento:\t{2:C}", FormatarHora(Pagamento.Horas, Horas.Minute), FormatarHora(hoje.Hour, hoje.Minute), Pagamento.Salario());
                IconeNotificacao.ShowBalloonTip(1000);
            }

            IconeNotificacao.BalloonTipTitle = "Timesheet";
            IconeNotificacao.BalloonTipText = "Dado inválido";
        }

        private void AbrirJanela()
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

                ExibirTooltipDeDados();
            }
        }

        private void Cronometro(object sender, System.Timers.ElapsedEventArgs e)
        {
            var db = new RegistroRepositorio();
            var hrsElapsed = Inactivity.GetLastInputTime();
            var ultimoRegistro = db.ObterUltimoRegistro();

            if (!Notificando && ultimoRegistro != null && ultimoRegistro.StatusUsuario == Registro.Usuario.Working)
            {
                var entrada = DateTime.Parse(ultimoRegistro.Dia + " " + ultimoRegistro.Entrada);
                var diferenca = DateTime.Now - entrada;
                var hoje = new DateTime().AddHours(Pagamento.Hoje) + diferenca;

                Dispatcher.Invoke(new Action(() =>
                {
                    var Horas = new DateTime().AddHours(Pagamento.Horas);
                    this.lblHrsHoje.Content = FormatarHora(hoje.Hour, hoje.Minute);
                    this.lblHrs.Content = FormatarHora(Pagamento.Horas, Horas.Minute);
                    this.lblValor.Content = string.Format("{0:C}", Pagamento.Salario());
                    this.lblMedia.Content = Pagamento.Media();
                }));

                if (hrsElapsed > Configuracao.TempoInativo * 60)
                {
                    Notificando = true;
                    Dispatcher.Invoke(new Action(() =>
                    {
                        AlertarSaida("O Sistema ficou inativo desde {0} deseja registrar como uma saída?", "O Sistema ficou inativo", string.Empty, true, DateTime.Parse(DateTime.Now.AddSeconds(-1 * hrsElapsed).ToString()));
                    }));

                    Notificando = false;
                }

                if (DateTime.Now >= DateTime.Parse("23:59:59"))
                {
                    temporizador.Elapsed -= Cronometro;

                    Registro.Sair(DateTime.Now, ultimoRegistro, this);
                    Thread.Sleep(1200);

                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Now, this));

                    temporizador.Elapsed += Cronometro;

                    db.SalvarAlteracao();
                }
            }

            db.Dispose();
        }

        private void SystemEvents_SessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            if (!entrada)
            {
                File.WriteAllText(Configuracao.Logs + "SwUser.log", DateTime.Now.ToString());
                entrada = true;
                temporizador.Elapsed -= Cronometro;
            }
            else
            {
                if (File.Exists(Configuracao.Logs + "SwUser.log"))
                    AlertarSaida("Foi registrado um logout as {0} deseja registrar como uma saída?", "Logout detectado", Configuracao.Logout);

                temporizador.Elapsed += Cronometro;
                entrada = false;
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            File.WriteAllText(Configuracao.Logs + "ShutUser.log", DateTime.Now.ToString());
            entrada = true;
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

            if (!Directory.Exists(Configuracao.Diretorio))
                Directory.CreateDirectory(Configuracao.Diretorio);

            if (!Directory.Exists(Configuracao.DiretorioBkp))
                Directory.CreateDirectory(Configuracao.DiretorioBkp);
            
            if (!File.Exists(Configuracao.RelatorioLogs))
                File.Create(Configuracao.RelatorioLogs);

            if (!Directory.Exists(Configuracao.Logs))
                Directory.CreateDirectory(Configuracao.Logs);

            if (!File.Exists(Configuracao.Atividades))
                File.Create(Configuracao.Atividades);

            if (!File.Exists(Configuracao.Relatorio))
            {
                File.WriteAllText(Configuracao.Relatorio, Registro.Cabecalho);
            }

            if (!Directory.Exists(Configuracao.PathConfig))
            {
                Directory.CreateDirectory(Configuracao.PathConfig);
                File.WriteAllLines(Configuracao.Config, Configuracao.ConfigFile.Split(';'));
            }
        }

        /// <summary>
        /// Verifica se existe algum log
        /// </summary>
        public void VerificarSaida()
        {
            var db = new RegistroRepositorio();

            if (File.Exists(Configuracao.Shutdown))
                AlertarSaida("O sistema foi desligado as {0} deseja registrar como uma saída?", "Shutdown detectado", Configuracao.Shutdown);

            else if(AnalisarLogSaida())
                AlertarSaida("O Sistema Timesheet ficou desativado desde {0} deseja registrar como uma saída?", "Incativadade do timesheet detectado", Configuracao.RelatorioLogs);

            else if (File.Exists(Configuracao.Logout))
                AlertarSaida("Foi registrado um logout as {0} deseja registrar como uma saída?", "Logout detectado", Configuracao.Logout);

            else if (db.ObterUltimoRegistro() == null)
            {
                this.Activate();
                this.Topmost = true;  // important
                Thread.Sleep(100);
                this.Focus();         // important
                this.Hide();

                var resultado = MessageBox.Show("Registrar entrada?", "Iniciando mês", MessageBoxButton.YesNo, MessageBoxImage.Question);
                this.Topmost = false; // important
                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {

                    var ultimaLinha = db.ObterUltimoRegistro();

                    if (ultimaLinha != null)
                        RegistrarFeriado(ultimaLinha.Dia);

                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Now, this));
                    db.SalvarAlteracao();
                }
            }

            db.Dispose();
        }

        /// <summary>
        /// Verifica se o log de saída representa uma saída válida o usuário apenas fechou e abriu o aplicativo ou reiniciou o computador
        /// </summary>
        /// <returns></returns>
        private bool AnalisarLogSaida()
        {
            var texto = File.ReadAllText(Configuracao.RelatorioLogs);
            DateTime data;

            if (DateTime.TryParse(texto, out data))
                return data.AddHours(1) < DateTime.Now;

            return false;
        }

        /// <summary>
        /// Exibe valores na tela
        /// </summary>
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

            var Horas = new DateTime().AddHours(Pagamento.Horas);
            this.lblHrs.Content = FormatarHora(Pagamento.Horas, Horas.Minute); ;
            lblValor.Content = string.Format("{0:C}",Pagamento.Salario());
            lblValorEsp.Content = string.Format("{0:C}", Pagamento.SalarioEsperado());
            lblMedia.Content = Pagamento.Media();
            lblHrsPretendidas.Content = Configuracao.HrsEsperadas.ToString();
            lblDiasUtes.Content = Pagamento.QuantidadeDiasUteis();
        }

        /// <summary>
        /// Recebe o ultimo registro e grava registros em branco até o dia de hoje
        /// </summary>
        /// <param name="ultimoDia">Ultimo dia cadastrado (linha completa)</param>
        private void RegistrarFeriado(string ultimoDia, RegistroRepositorio dbInstance = null)
        {
            var db = dbInstance == null ? new RegistroRepositorio() : dbInstance;
            var dia = ultimoDia.Split('/');
            if(dia.Length > 1)
            {
                var UltimoDiaRegistrado = Convert.ToInt32(dia[0]) + 1;
                var Hoje = DateTime.Now.Day;

                while (UltimoDiaRegistrado < Hoje)
                {
                    var registro = new Registro();

                    registro.Dia = DateTime.Parse(UltimoDiaRegistrado.ToString() + '/' + DateTime.Now.Month.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    registro.Entrada = "";
                    registro.StatusEntrada = "";
                    registro.Saida = "";
                    registro.StatusSaida = "";

                    db.ListarRegistros().Add(registro);

                    registro = new Registro();

                    registro.Dia = DateTime.Parse(UltimoDiaRegistrado.ToString() + '/' + DateTime.Now.Month.ToString()).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    registro.Entrada = "";
                    registro.StatusEntrada = "";
                    registro.Saida = "";
                    registro.StatusSaida = "";

                    db.ListarRegistros().Add(registro);

                    UltimoDiaRegistrado++;
                }
            }
            if (dbInstance == null)
            {
                db.SalvarAlteracao();
                db.Dispose();
            }
        }

        /// <summary>
        /// Abre o MessageBox e pega o retorno do click do usuário
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="titulo"></param>
        /// <param name="path"></param>
        /// <param name="elapsed"></param>
        /// <param name="data"></param>
        public void AlertarSaida(string msg, string titulo, string path, bool elapsed = false, DateTime data = new DateTime())
        {
            try
            {
                var db = new RegistroRepositorio();

                this.WindowState = System.Windows.WindowState.Normal;
                this.Activate();
                this.Topmost = false; // important
                this.Topmost = true;  // important
                Thread.Sleep(100);
                this.ShowInTaskbar = true;
                this.Focus();         // important
                this.Hide();

                string dataSaida;
                if (string.IsNullOrEmpty(path))
                    dataSaida = data.ToString();
                else
                {
                    dataSaida = File.ReadAllLines(path)[0];
                    data = DateTime.Now;
                }
                temporizador.Elapsed -= Cronometro;
                SystemEvents.SessionSwitch -= SystemEvents_SessionSwitch;

                var resultado = MessageBox.Show(string.Format(msg, dataSaida), titulo, MessageBoxButton.YesNo, MessageBoxImage.Question);

                this.Topmost = false; // important
                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {
                    var ultimoRegistro = db.ObterUltimoRegistro();
                    Registro.Sair(DateTime.Parse(dataSaida), ultimoRegistro, this);
                    Registro entrada;

                    RegistrarFeriado(DateTime.Parse(dataSaida).ToString("dd/MM/yyyy"), db);

                    if (!elapsed)
                        entrada = Registro.Entrar(data, this);
                    else
                    {
                        entrada = Registro.Entrar(DateTime.Now, this);
                    }

                    db.ListarRegistros().Add(entrada);
                    db.SalvarAlteracao();

                }
                else if (DateTime.Now >= DateTime.Parse(DateTime.Parse(dataSaida).ToShortDateString() + " 23:59:59") && elapsed)
                {

                    var ultimoRegistro = db.ObterUltimoRegistro();
                    Registro.Sair(DateTime.Parse(DateTime.Parse(dataSaida).ToShortDateString() + " 23:59:59"), ultimoRegistro, this);
                    Thread.Sleep(1200);
                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Parse(DateTime.Parse(dataSaida).AddDays(1).ToShortDateString() + " 00:00:01"), this));
                    db.SalvarAlteracao();
                }

                if (!string.IsNullOrEmpty(path))
                    File.Delete(path);

                db.Dispose();
                
                temporizador.Elapsed += Cronometro;
                SystemEvents.SessionSwitch += SystemEvents_SessionSwitch;

            }
            catch (IOException ioExc)
            {
                MessageBox.Show(ioExc.Message);
            }
        }

        /// <summary>
        /// Retorna uma hora em um formato válido
        /// </summary>
        /// <param name="hora"></param>
        /// <param name="minuto"></param>
        /// <returns></returns>
        private string FormatarHora(double hora, int minuto)
        {
            var _hora = Convert.ToInt32(Math.Floor(hora)) < 10 ? "0" + Convert.ToInt32(Math.Floor(hora)).ToString() : Convert.ToInt32(Math.Floor(hora)).ToString();
            var _minuto = minuto < 10 ? "0" + minuto.ToString() : minuto.ToString();

            return _hora + ":" + _minuto;
        }
    }
}
