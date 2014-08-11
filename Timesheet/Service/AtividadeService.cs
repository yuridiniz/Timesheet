using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using Timesheet.Model;
using Timesheet.Repositorio;

namespace Timesheet.Service
{
    /// <summary>
    /// Serviço de solicitação registro de atividades ao usuário
    /// </summary>
    class AtividadeService : BaseService
    {
        protected CadastrarAtividade CadastroAtividade;

        public AtividadeService()
            : base(60 * 60 * 3)
        {
        }

        protected override void Acao(object sender, System.Timers.ElapsedEventArgs e)
        {
            var db = new RegistroRepositorio();
            var hrsElapsed = Inactivity.GetLastInputTime();
            var ultimoRegistro = db.ObterUltimoRegistro();

            if (ultimoRegistro != null && ultimoRegistro.StatusUsuario == Registro.Usuario.Working)
            {
                App.Current.Dispatcher.Invoke((Action)(() =>
                {
                    var desktopWorkingArea = System.Windows.SystemParameters.WorkArea;

                    if (CadastroAtividade != null)
                        CadastroAtividade.Close();

                    CadastroAtividade = new CadastrarAtividade();
                    CadastroAtividade.Topmost = true;
                    CadastroAtividade.WindowStartupLocation = WindowStartupLocation.Manual;
                    CadastroAtividade.Left = desktopWorkingArea.Right - CadastroAtividade.Width;
                    CadastroAtividade.Top = desktopWorkingArea.Bottom - CadastroAtividade.Height;
                    CadastroAtividade.ShowInTaskbar = false;
                    CadastroAtividade.ShowDialog();
                }));

            }
        }
    }
}
