using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public class Registro
    {
        public string Id { get; set; }
        public string Dia { get; set; }
        public string Entrada { get; set; }
        public string Saida { get; set; }
        public string Atividade { get; set; }
        public string StatusEntrada { get; set; }
        public string StatusSaida { get; set; }
        public Usuario StatusUsuario
        {
            get
            {
                if(string.IsNullOrEmpty(Saida) && string.IsNullOrEmpty(Entrada))
                    return Usuario.Feriado;
                else if (string.IsNullOrEmpty(Saida))
                    return Usuario.Working;
                else
                    return Usuario.Off;
            }
        }
        public double TotalHoras
        {
            get
            {
                DateTime entrada = new DateTime();
                DateTime saida;

                if (StatusUsuario == Usuario.Feriado)
                    return 0;
                else if (StatusUsuario == Usuario.Working)
                {
                    entrada = Convert.ToDateTime(Entrada);
                    saida = DateTime.Now;
                }
                else
                {
                    entrada = Convert.ToDateTime(Entrada);
                    saida = Convert.ToDateTime(Saida);
                }

                return (saida - entrada).TotalHours;
            }
        }


        public enum Usuario
        {
            Working,
            Feriado,
            Off
        }

        /// <summary>
        /// Propriedade para manter a compatibilidade com a versão atual, essa propriedade irá ser substituida para "StatusEntrada" e "StatusSaida"
        /// </summary>
        public string Conferir { get; set; }
    }
}
