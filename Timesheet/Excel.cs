using System;
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
using Timesheet.Repositorio;
using System.Drawing;

namespace Timesheet
{
    public class Excel
    {
        #region Métodos do Excel

        public static void CriarExcel()
        {
            try
            {
                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                    Process.GetProcessesByName("EXCEL")[i].Kill();

                SaveFileDialog dialogo = new SaveFileDialog();
                dialogo.Filter = "Excel Files | *.xlsx";
                dialogo.DefaultExt = "txt";
                dialogo.ShowDialog();

                var timesheetExcel = dialogo.FileName;

                if (string.IsNullOrEmpty(timesheetExcel))
                    return;

                var exApp = new Microsoft.Office.Interop.Excel.Application();
                Workbook work = exApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                Worksheet excelWorksheet = (Worksheet)work.Worksheets[1];

                exApp.Visible = true;

                RegistroRepositorio rep = new RegistroRepositorio();
                var lista = rep.ListarRegistros();

                Range tudo = (Range)excelWorksheet.get_Range("A1", "Z200");
                tudo.Font.Name = "Myriad Web Pro";
                tudo.Font.Size = 12;
                tudo.HorizontalAlignment = XlHAlign.xlHAlignCenter;
                tudo.VerticalAlignment = XlVAlign.xlVAlignCenter;

                var listaAgrupada = lista.GroupBy(p => p.Dia).ToList();
                var indiceInicial = 1;
                var indiceFinal = 0;
                var indice = indiceInicial;

                foreach (var item in listaAgrupada)
                {
                    indice = indiceInicial;
                    if (item.FirstOrDefault().DiaDaSemana == Registro.Semana.Domingo)
                    {
                        CriarLinhaTotalizadora(indice, excelWorksheet);
                        indice++;
                    }

                    indiceFinal = indice + item.Count() - 1;
                    var rengeA = excelWorksheet.get_Range("A" + indice, "A" + indiceFinal);
                    rengeA.Merge();
                    rengeA.Value = DateTime.Parse(item.FirstOrDefault().Dia);
                    FormatarRange(rengeA, item.FirstOrDefault().DiaDaSemana, "dd/mmm", 9);

                    var rengeB = excelWorksheet.get_Range("B" + indice, "B" + indiceFinal);
                    rengeB.Merge();
                    rengeB.Value = item.FirstOrDefault().TextoSemana;
                    FormatarRange(rengeB, item.FirstOrDefault().DiaDaSemana, "", 15);

                    var rengeC = excelWorksheet.get_Range("C" + indice, "C" + indiceFinal);
                    rengeC.Merge();
                    rengeC.Formula = string.Format("=SUM(F{0}:F{1})", indice, indiceFinal);
                    FormatarRange(rengeC, item.FirstOrDefault().DiaDaSemana, "hh:mm", 11);

                    foreach (var dados in item.ToList())
                    {
                        Range cellEntrada = (Range)excelWorksheet.get_Range("D" + indice, "D" + indice);
                        Range cellSaida = (Range)excelWorksheet.get_Range("E" + indice, "E" + indice);
                        var Soma = excelWorksheet.get_Range("F" + indice, "F" + indice);
                        Range cellProjeto = (Range)excelWorksheet.get_Range("G" + indice);
                        Range cellDesc = (Range)excelWorksheet.get_Range("H" + indice);

                        FormatarRange(cellEntrada, dados.DiaDaSemana, "hh:mm", 9);
                        FormatarRange(cellSaida, dados.DiaDaSemana, "hh:mm", 9);
                        FormatarRange(Soma, dados.DiaDaSemana, "hh:mm", 8);
                        FormatarRange(cellProjeto, dados.DiaDaSemana, "", 8);
                        FormatarRange(cellDesc, dados.DiaDaSemana, "hh:mm", 60);

                        cellEntrada.Value = dados.Entrada;
                        cellSaida.Value = dados.Saida;

                        if (!string.IsNullOrEmpty(dados.Entrada) && !string.IsNullOrEmpty(dados.Saida))
                            Soma.Formula = string.Format("= E{0}-D{0}", indice);

                        indice++;
                    }

                    indiceInicial = indiceFinal + 1;
                }

                CriarLinhaTotalizadora(indiceFinal + 1, excelWorksheet);
                work.SaveAs(timesheetExcel);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante o processo de exportação!");

            }
        }

        public static void ExportarExcel()
        {
            try
            {
                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                    Process.GetProcessesByName("EXCEL")[i].Kill();

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
                            Thread.Sleep(100);
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
                                var desc = dados.Length == 6 ? dados[5] : "";

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
                    sr.Close();
                }
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

        /// <summary>
        /// Cria a linha totalizadora no final de cada semana
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="excelWorksheet"></param>
        private static void CriarLinhaTotalizadora(int indice, Worksheet excelWorksheet)
        {
            var rengeTotalDaSemana = excelWorksheet.get_Range("A" + indice, "D" + indice);
            var rengeValor = excelWorksheet.get_Range("E" + indice, "E" + indice);
            var Renge1 = excelWorksheet.get_Range("F" + indice, "F" + indice);
            var Renge2 = excelWorksheet.get_Range("G" + indice, "G" + indice);
            var Renge3 = excelWorksheet.get_Range("H" + indice, "H" + indice);

            rengeTotalDaSemana.Merge();
            rengeValor.Merge();
            FormatarRange(rengeTotalDaSemana, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(rengeValor, Registro.Semana.Segunda, "hh:mm", 0, true);
            FormatarRange(Renge1, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(Renge2, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(Renge3, Registro.Semana.Segunda, "", 0, true);
        }

        /// <summary>
        /// Estiliza a celula
        /// </summary>
        /// <param name="range"></param>
        /// <param name="formato"></param>
        private static void FormatarRange(Range range, Registro.Semana diaDaSemana, string formato = null, int width = 0, bool isTotalizador = false)
        {
             range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Color = Color.Black.ToArgb();

            if (!string.IsNullOrEmpty(formato))
                range.NumberFormat = formato;

            if (width > 0)
                range.ColumnWidth = width;

            if (diaDaSemana == Registro.Semana.Domingo
               || diaDaSemana == Registro.Semana.Sabado)
            {
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
            }

            if (isTotalizador)
            {
                range.Font.Bold = true;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("8db4e2", 16)));
                range.Value = "teste";
            }


        }

        #endregion
    }
}
