using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace Timesheet.Service
{
    public abstract class BaseService
    {
        private int segundo = 1000;
        private System.Timers.Timer temporizador = new System.Timers.Timer();

        protected BaseService(int segundos)
        {
            temporizador.Interval = segundo * segundos;
            temporizador.Elapsed += Acao;
            temporizador.Start();
        }

        protected abstract void Acao(object sender, System.Timers.ElapsedEventArgs e);
    }
}
