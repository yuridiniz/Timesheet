﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Timesheet.Model;

namespace Timesheet
{
    public class Excel
    {
        #region Métodos do Excel

        public static void ExportarExcel()
        {
            try
            {
                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                    Process.GetProcessesByName("EXCEL")[i].Kill();

                //var desc = new EXCEL.Workbook();
                OpenFileDialog dialogo = new OpenFileDialog();
                dialogo.ShowDialog();

                var timesheetExcel = dialogo.FileName;

                if (string.IsNullOrEmpty(timesheetExcel))
                    return;

                var exApp = new Microsoft.Office.Interop.Excel.Application();
                var work = exApp.Workbooks.Open(timesheetExcel, 0, false, 5, "", "", false, XlPlatform.xlWindows, "", true, false, 0, true, false, false);
                exApp.Visible = true;

                Worksheet excelWorksheet = null;
                foreach (Worksheet worksheet in work.Worksheets)
                    excelWorksheet = worksheet;

                if (excelWorksheet == null)
                    return;

                using (StreamReader sr = new StreamReader(Configuracao.Relatorio))
                {
                    var linha = sr.ReadLine();
                    linha = sr.ReadLine();
                    int linhaEditavel = 1;
                    int linhaEditavelAnterior = linhaEditavel;
                    while (linha != null)
                    {
                        var dados = linha.Split(';');
                        if (!string.IsNullOrWhiteSpace(dados[3]) && dados.Length > 4)
                        {
                            Thread.Sleep(2500);
                            linhaEditavel = ObterLinhaDaData(Convert.ToDateTime(dados[0]), excelWorksheet);
                            if (linhaEditavel == linhaEditavelAnterior)
                                linhaEditavel++;
                            else if (linhaEditavel < linhaEditavelAnterior)
                            {
                                var diferenca = linhaEditavelAnterior - linhaEditavel;
                                linhaEditavel += (diferenca + 1);
                            }

                            if (linhaEditavel != -1)
                            {
                                var entrada = Convert.ToDateTime(dados[1]);
                                var saida = Convert.ToDateTime(dados[3]);
                                var desc = dados[5];

                                var totalHrs = (saida - entrada).TotalHours;

                                Range cellEntrada = (Range)excelWorksheet.get_Range("D" + linhaEditavel, "D" + linhaEditavel);
                                Range cellSaida = (Range)excelWorksheet.get_Range("E" + linhaEditavel, "E" + linhaEditavel);
                                Range cellDesc = cellDesc = (Range)excelWorksheet.get_Range("H" + linhaEditavel, "H" + linhaEditavel);

                                cellEntrada.Value = entrada.ToShortTimeString();
                                cellSaida.Value = saida.ToShortTimeString();
                                cellDesc.Value = desc;

                                //cellEntrada.Value = "";
                                //cellSaida.Value = "";
                                //cellDesc.Value = "";

                                linhaEditavelAnterior = linhaEditavel;
                            }

                        }

                        linha = sr.ReadLine();
                    }

                    work.Save();
                    work.Close();
                    sr.Close();
                }

                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                {
                    Process.GetProcessesByName("EXCEL")[i].Kill();
                }

                Process.Start(timesheetExcel);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante o processo de exportação!");

            }
        }

        /// <summary>
        /// Busca qual linha do excel é referente a uma data
        /// </summary>
        /// <param name="data">Data a ser encontrada no excel</param>
        /// <param name="excelWorksheet">worksheet do excel</param>
        /// <returns>número da linha</returns>
        public static int ObterLinhaDaData(DateTime data, Worksheet excelWorksheet)
        {
            var dataLinha = new DateTime();
            int startIndex = 0;
            while (data.ToShortDateString() != dataLinha.ToShortDateString())
            {
                startIndex++;

                Range cellRotulo = (Range)excelWorksheet.get_Range("A" + startIndex, "A" + startIndex);

                if (cellRotulo.Value != null)
                {
                    var valor = Convert.ToString(cellRotulo.Value);
                    var dataLinha2 = Convert.ToDateTime(dataLinha.ToShortDateString());
                    var parse = DateTime.TryParse(valor, out dataLinha2);
                    if (parse == true)
                        dataLinha = dataLinha2;
                }

                if (startIndex == 100)
                    return -1;

            }

            return startIndex;
        }

        #endregion
    }
}