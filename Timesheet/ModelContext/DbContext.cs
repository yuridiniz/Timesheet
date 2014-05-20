using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;

namespace Timesheet.ModelContext
{
    public class DbContext : IDisposable
    {
        public List<Registro> Registros;
        public List<Atividade> Atividades;
        public List<Log> Logs;

        public DbContext()
        {
            Registros = new List<Registro>();
            string[] linhasRelatorio = File.ReadAllLines(Configuracao.Relatorio);

            Registros = ParseTxtToList<Registro>(linhasRelatorio);
        }

        /// <summary>
        /// Salva alterações do contexto direto no arquivo TXT
        /// </summary>
        public void SalvarAlteracao()
        {
            ParseListToTxt();
        }

        private string[] ObterCabecalho(string[] linhas)
        {
            if (linhas.Length > 0)
                return linhas[0].Split(';');

            return null;
        }

        private List<TArquivo> ParseTxtToList<TArquivo>(string[] linhas) where TArquivo : new()
        {
            List<TArquivo> tabela = new List<TArquivo>();

            string[] cabecalho = typeof(TArquivo).GetProperty("Cabecalho").GetValue(null).ToString().Split(';');

            for (var i = 1; i < linhas.Length; i++)
            {
                var row = linhas[i];
                var calulas = row.Split(';');
                TArquivo registro = new TArquivo();

                for (var c = 0; c < (calulas.Length > 5 ? 5 : calulas.Length); c++)
                {
                    var prop = registro.GetType().GetProperty(cabecalho[c]);
                    if (calulas[c].Trim().Contains('/') && !calulas[c].Trim().Contains("2014"))
                        prop.SetValue(registro, calulas[c].Trim()+"/2014");
                    else
                        prop.SetValue(registro, calulas[c].Trim());
                }

                tabela.Add(registro);
            }
            
            return tabela;
        }

        private void ParseListToTxt()
        {
            List<string> texto = new List<string>();
            if (Registros.Count > 0)
            {
                texto.Add(Registro.Cabecalho);
                foreach (var reg in Registros)
                {
                    texto.Add(reg.Dia + ";" +
                        reg.Entrada + ";" +
                        reg.StatusEntrada + ";" +
                        reg.Saida + ";" +
                        reg.StatusSaida);
                }
            }

            File.WriteAllLines(Configuracao.Relatorio, texto.ToArray());
        }

        public void Dispose()
        {
            Registros = null;
        }
    }
}
