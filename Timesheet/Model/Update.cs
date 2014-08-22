using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public class Update
    {
        public string Versao { get; set; }
        public bool UpdateExe { get; set; }
        public List<string> Descricao { get; set; }
    }
}
