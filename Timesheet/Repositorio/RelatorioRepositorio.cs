using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;
using Timesheet.ModelContext;

namespace Timesheet.Repositorio
{
    public class RegistroRepositorio : IDisposable
    {
        private DbContext db;

        public RegistroRepositorio()
        {
            db = new DbContext();
        }
        public List<Registro> ListarRegistros()
        {
            var resultado = db.Registros;
            return resultado;
        }

        public Registro ObterUltimoRegistro()
        {
            var resultado = db.Registros.LastOrDefault(p => p.StatusUsuario != Registro.Usuario.Feriado);
            return resultado;
        }

        public void SalvarAlteracao()
        {
            db.SalvarAlteracao();
        }

        public void Dispose()
        {
            db.Dispose();
            db = null;
        }
    }
}
