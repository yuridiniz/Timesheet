using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Timesheet.Model;

namespace Timesheet.Base
{
    public static class DB
    {
        public static DataTable registros;

        public static void CarregarTabela()
        {
            using (StreamReader sr = new StreamReader(Configuracao.Path))
            {
                var linha = sr.ReadLine();
                var dados = linha.Split(';');

                registros = new DataTable();
                registros.TableName = "Registro";
                registros.Columns.Add("Id", typeof(Int32));

                for (var i = 0; i < dados.Length; i++)
                    registros.Columns.Add(dados[i], typeof(String));


                linha = sr.ReadLine();
                DataRow dadoLinha;
                var cont = 1;
                while (linha != null)
                {
                    dados = (cont.ToString() + " ; " + linha).Split(';');
                    
                    dadoLinha = registros.NewRow();
                    dadoLinha.ItemArray = dados;
                    registros.Rows.Add(dadoLinha);

                    linha = sr.ReadLine();

                    cont++;
                }

                sr.Close();

                var query =
                    from order in registros.AsEnumerable()
                    where order.Field<string>("Status_Saida").Trim() == "True"
                    select new
                    {
                        SalesOrderID = order.Field<string>("Atividade"),
                    };

                var orderTable = query.ToList(); 
            }

        }
    }
}
