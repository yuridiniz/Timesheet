using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Update
{
    //Substitui os arquivos
    class Program
    {
        static void Main(string[] args)
        {
            string parametros = string.Empty;
            string[] diretorio = new string[2];

            Console.WriteLine("Aguardando construção de binarios...");
            Thread.Sleep(1000);
            Console.WriteLine("Carregando dados em lote...");
            Thread.Sleep(2000);
            Console.WriteLine("Construção em processo...");

            for (var i = 0; i < args.Length; i++)
                parametros += args[i] + " ";

            parametros = parametros.Substring(parametros.IndexOf("--pathData:"), parametros.Length - parametros.IndexOf("--pathData:"));
            diretorio = parametros.Trim().Replace("--pathData:", "").Split('|');

            if (!string.IsNullOrEmpty(diretorio[0]))
            {
                
                Console.WriteLine("Dir0: " + diretorio[0]);
                Console.WriteLine("Dir1: " + diretorio[1]);

                while (!File.Exists(diretorio[1] + "\\TimesheetNew.exe")) ;

                File.Delete(diretorio[0] + "\\Timesheet.exe");
                File.Move(diretorio[1] + "\\TimesheetNew.exe", diretorio[0] + "\\Timesheet.exe");

                Console.WriteLine("Instalação completa!");

                Process.Start(diretorio[0] + "\\Timesheet.exe");
                return;
            }

            Console.WriteLine("Argumentos não foram passados corretamente, entre em contato para mais informações...");
            
        }
    }
}
