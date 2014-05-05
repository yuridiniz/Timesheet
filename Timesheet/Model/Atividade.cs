using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public class Atividade
    {
        public string Descricao { get; set; }
        private string Data { get; set; }

        public Atividade()
        {
        }

        public Atividade(string descricao)
        {
            this.Descricao = descricao;
        }

        public void Registrar()
        {
            Data = DateTime.Now.ToString();
            GravarEmArquivo();
        }

        public static void Registrar(string descricao)
        {
            var atividade = new Atividade(descricao);
            atividade.Registrar();
        }

        private void GravarEmArquivo()
        {
            using (var sw = new StreamWriter(Configuracao.Atividades, true))
            {
                sw.WriteLine(this.Data + ";" + this.Descricao);
                sw.Close();
            }
        }
    }
}
