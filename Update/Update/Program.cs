using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace Update
{
    //Substitui os arquivos
    class Program
    {
        static void Main(string[] args)
        {
            var diretorio = args[0].Split('&');

            while (!File.Exists(diretorio[1] + "\\TimesheetNew.exe")) ;

            File.Delete(diretorio[0] + "\\Timesheet.exe");
            File.Move(diretorio[1] + "\\TimesheetNew.exe", diretorio[0] + "\\Timesheet.exe");

            Process.Start(diretorio[0] + "\\Timesheet.exe");
        }
    }
}
