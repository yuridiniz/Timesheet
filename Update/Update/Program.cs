using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Update
{
    class Program
    {
        static void Main(string[] args)
        {
            File.Delete(args[0] + "\\Timesheet.exe");
            File.Move(Environment.CurrentDirectory + "\\TimesheetNew.exe", args[0] + "\\Timesheet.exe");
        }
    }
}
