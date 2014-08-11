using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;

namespace Timesheet.Service
{
    /// <summary>
    /// Serviço de registro de saída
    /// </summary>
    class LogSaidaService : BaseService
    {
        
        public LogSaidaService()
            : base(60)
        {

        }

        protected override void Acao(object sender, System.Timers.ElapsedEventArgs e)
        {
            File.WriteAllText(Configuracao.RelatorioLogs, DateTime.Now.ToString());
        }
    }
}
