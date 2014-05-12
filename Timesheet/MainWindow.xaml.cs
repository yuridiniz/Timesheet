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

namespace Timesheet
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        bool entrada = false;
        System.Timers.Timer temporizador;
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

            VerificarSaida();
            ExibirValores();

            temporizador = new System.Timers.Timer();
            temporizador.Interval = 1000;
            temporizador.Elapsed += Cronometro;
            temporizador.Start();

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

        #region Eventos de Usuário

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

        private void Cronometro(object sender, System.Timers.ElapsedEventArgs e)
        {
            var db = new RegistroRepositorio();
            var hrsElapsed = Inactivity.GetLastInputTime();
            var ultimoRegistro = db.ObterUltimoRegistro();

            if (!Notificando && ultimoRegistro != null && ultimoRegistro.StatusUsuario == Registro.Usuario.Working)
            {
                var entrada = DateTime.Parse(ultimoRegistro.Dia + " " + ultimoRegistro.Entrada);
                var diferenca = DateTime.Now - entrada;

                Dispatcher.Invoke(new Action(() =>
                {
                    var totalHrs = Pagamento.Horas + diferenca.TotalSeconds / (60 * 60);
                    var Horas = new DateTime().AddHours(totalHrs);
                    this.lblHrs.Content = FormatarHora(totalHrs, Horas.Minute); ;
                    this.lblValor.Content = string.Format("{0:C}", (Convert.ToInt32(Pagamento.Salario()) + Configuracao.ValorHr * (diferenca.TotalSeconds / (60 * 60))));
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

                    Registro.Sair(DateTime.Now.AddMinutes(-4), ultimoRegistro, this);
                    Thread.Sleep(1200);

                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Now.AddMinutes(4), this));

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
                temporizador.Elapsed += Cronometro;
                if (File.Exists(Configuracao.Logs + "SwUser.log"))
                    AlertarSaida("Foi registrado um logout as {0} deseja registrar como uma saída?", "Logout detectado", Configuracao.Logout);

                entrada = false;
            }
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            File.WriteAllText(Configuracao.Logs + "ShutUser.log", DateTime.Now.ToString());
            entrada = true;
        }

        private void btnEntrada_Click(object s, EventArgs e)
        {
            using (var db = new RegistroRepositorio())
            {
                var novoRegistro = Registro.Entrar(DateTime.Now, this);
                db.ListarRegistros().Add(novoRegistro);
                db.SalvarAlteracao();

                btnEntrada.IsEnabled = false;
                btnSair.IsEnabled = true;
            }
            
        }

        private void btnSair_Click(object s, EventArgs e)
        {
            using (var db = new RegistroRepositorio())
            {
                var ultimoRegistro = db.ObterUltimoRegistro();
                Registro.Sair(DateTime.Now, ultimoRegistro, this);

                db.SalvarAlteracao();

                btnEntrada.IsEnabled = true;
                btnSair.IsEnabled = false;
            }
        }

        private void btnExportar_Click(object s, EventArgs e)
        {
            Task.Run(() => Excel.ExportarExcel());
        }

        private void btnConfig_Click(object s, EventArgs e)
        {
            System.Diagnostics.Process.Start(Configuracao.Diretorio);
        }

        #endregion

        #region Métodos Gerais
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
                File.WriteAllText(Configuracao.Config, Configuracao.ConfigFile);
            }

            if (File.Exists(Configuracao.Relatorio))
            {
                try
                {
                    var db = new RegistroRepositorio();

                    var ultimaLinha = db.ObterUltimoRegistro();

                    if (ultimaLinha != null)
                    {
                        RegistrarFeriado(ultimaLinha.Dia);
                        AplicarEstadoBtn(ultimaLinha);
                    }

                    db.Dispose();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
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

            else if (File.Exists(Configuracao.Logout))
                AlertarSaida("Foi registrado um logout as {0} deseja registrar como uma saída?", "Logout detectado", Configuracao.Logout);

            else if (db.ObterUltimoRegistro() == null)
            {
                this.Hide();
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important

                var resultado = MessageBox.Show("Registrar entrada?", "Iniciando mês", MessageBoxButton.YesNo, MessageBoxImage.Question);

                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {
                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Now, this));
                    db.SalvarAlteracao();
                }
            }

            db.Dispose();
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

            var Horas = new DateTime().AddHours(Pagamento.Horas / (60 * 60));
            lblHrs.Content = Horas.ToShortTimeString();
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
        private void RegistrarFeriado(string ultimoDia)
        {
            var db = new RegistroRepositorio();
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

                    UltimoDiaRegistrado++;
                }
            }
            db.SalvarAlteracao();
            db.Dispose();
        }

        /// <summary>
        /// Verifica qual foi a ultima ação salva e habilita o botão para dar seguencia aos registros
        /// </summary>
        /// <param name="ultimaLinha"></param>
        public void AplicarEstadoBtn(Registro registro)
        {
            if (registro.StatusUsuario == Registro.Usuario.Working)
            {
                btnEntrada.IsEnabled = false;
                btnSair.IsEnabled = true;
            }
        }

        public void AlertarSaida(string msg, string titulo, string path, bool elapsed = false, DateTime data = new DateTime())
        {
            try
            {
                var db = new RegistroRepositorio();

                this.Hide();
                this.Activate();
                this.Topmost = true;  // important
                this.Topmost = false; // important
                this.Focus();         // important

                string dataSaida;

                if (string.IsNullOrEmpty(path))
                    dataSaida = data.ToString();
                else
                {
                    dataSaida = File.ReadAllLines(path)[0];
                    data = DateTime.Now;
                }

                var resultado = MessageBox.Show(string.Format(msg, dataSaida), titulo, MessageBoxButton.YesNo, MessageBoxImage.Question);

                this.Show();

                if (resultado == MessageBoxResult.Yes)
                {
                    var ultimoRegistro = db.ObterUltimoRegistro();
                    Registro.Sair(DateTime.Now, ultimoRegistro, this);
                    Registro entrada;

                    if (!elapsed)
                        entrada = Registro.Entrar(data, this);
                    else
                        entrada = Registro.Entrar(DateTime.Now, this);

                    db.ListarRegistros().Add(entrada);
                }
                else if (DateTime.Now >= DateTime.Parse(DateTime.Parse(dataSaida).ToShortDateString() + " 23:59:59") && elapsed)
                {
                    temporizador.Elapsed -= Cronometro;

                    var ultimoRegistro = db.ObterUltimoRegistro();
                    Registro.Sair(DateTime.Now.AddMinutes(-4), ultimoRegistro, this);
                    Thread.Sleep(1200);
                    db.ListarRegistros().Add(Registro.Entrar(DateTime.Now.AddMinutes(4), this));

                    temporizador.Elapsed += Cronometro;
                }

                if (!string.IsNullOrEmpty(path))
                    File.Delete(path);

                db.SalvarAlteracao();
                db.Dispose();
            }
            catch (IOException ioExc)
            {
                MessageBox.Show(ioExc.Message);
            }
        }

        private string FormatarHora(double hora, int minuto)
        {
            var _hora = Convert.ToInt32(Math.Floor(hora)) < 10 ? "0" + Convert.ToInt32(Math.Floor(hora)).ToString() : Convert.ToInt32(Math.Floor(hora)).ToString();
            var _minuto = minuto < 10 ? "0" + minuto.ToString() : minuto.ToString();

            return _hora + ":" + _minuto;
        }

        #endregion
    }
}
