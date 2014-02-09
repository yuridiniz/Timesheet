using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Timesheet.Model
{
    public static class Configuracao
    {
        public static string Diretorio = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/";
        public static string PathConfig = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config";
        public static string Config = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Config/Config.ini";
        public static string Path = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "/Timesheet/Relatorio.txt";
    }
}
