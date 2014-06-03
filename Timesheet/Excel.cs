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
    /// <summary>
    /// Métodos do excel
    /// </summary>
    public class Excel
    {
        private static Worksheet ExcelWorksheet;

        /// <summary>
        /// Cria um modelo e preenche a planilha
        /// </summary>
        public static void CriarExcel()
        {
            try
            {
                for (var i = 0; i < Process.GetProcessesByName("EXCEL").Length; i++)
                    Process.GetProcessesByName("EXCEL")[i].Kill();

                SaveFileDialog dialogo = new SaveFileDialog();
                dialogo.Filter = "Arquivo Excel | *.xlsx";
                dialogo.DefaultExt = "txt";
                dialogo.ShowDialog();

                if (string.IsNullOrEmpty(dialogo.FileName))
                    return;

                var exApp = new Microsoft.Office.Interop.Excel.Application();
                Workbook work = exApp.Workbooks.Add(XlWBATemplate.xlWBATWorksheet);
                ExcelWorksheet = (Worksheet)work.Worksheets[1];
                exApp.Visible = true;

                IniciarConstrucao();
                
                work.SaveAs(dialogo.FileName);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Ocorreu um erro durante o processo de exportação!");
            }
        }

        /// <summary>
        /// Inicia o processo para preenchimento dos dados
        /// </summary>
        /// <param name="excelWorksheet"></param>
        private static void IniciarConstrucao()
        {
            RegistroRepositorio rep = new RegistroRepositorio();
            var lista = rep.ListarRegistros();

            Range tudo = ObterRange("A1", "Z200");
            tudo.Font.Name = "Myriad Web Pro";
            tudo.Font.Size = 12;
            tudo.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            tudo.VerticalAlignment = XlVAlign.xlVAlignCenter;

            CriarCabecalhoDoArquivo();
            CriarCabecalhoTabela();

            var listaAgrupada = lista.GroupBy(p => p.Dia).ToList();
            int indiceInicial = 5;
            int indice = indiceInicial;
            int totalizadorInicio = indiceInicial;
            int indiceFinal = 0;

            foreach (var item in listaAgrupada)
            {
                indice = indiceInicial;
                if (item.FirstOrDefault().DiaDaSemana == Registro.Semana.Domingo)
                {
                    CriarLinhaTotalizadora(indice++, totalizadorInicio);
                    totalizadorInicio = indice;
                }

                int merge = item.Count() == 1 ? 2 : item.Count();
                indiceFinal = indice + merge - 1;
                Range rengeA = ObterRange("A" + indice, "A" + indiceFinal);
                rengeA.Merge();
                rengeA.Value = DateTime.Parse(item.FirstOrDefault().Dia);
                FormatarRange(rengeA, item.FirstOrDefault().DiaDaSemana, "dd/mmm", 9);

                Range rengeB = ObterRange("B" + indice, "B" + indiceFinal);
                rengeB.Merge();
                rengeB.Value = item.FirstOrDefault().TextoSemana;
                FormatarRange(rengeB, item.FirstOrDefault().DiaDaSemana, "", 15);

                Range rengeC = ObterRange("C" + indice, "C" + indiceFinal);
                rengeC.Merge();
                rengeC.Formula = string.Format("=SUM(F{0}:F{1})", indice, indiceFinal);
                FormatarRange(rengeC, item.FirstOrDefault().DiaDaSemana, "hh:mm", 11);

                foreach (var dados in item.ToList())
                {
                    if (item.Count() == 1)
                    {
                        Range cellEntrada1 = ObterRange("D" + (indice + 1));
                        Range cellSaida1 = ObterRange("E" + (indice + 1));
                        Range Soma1 = ObterRange("F" + (indice + 1));
                        Range cellProjeto1 = ObterRange("G" + (indice + 1));
                        Range cellDesc1 = ObterRange("H" + (indice + 1));

                        FormatarRange(cellEntrada1, dados.DiaDaSemana, "hh:mm", 9);
                        FormatarRange(cellSaida1, dados.DiaDaSemana, "hh:mm", 9);
                        FormatarRange(Soma1, dados.DiaDaSemana, "hh:mm", 8);
                        FormatarRange(cellProjeto1, dados.DiaDaSemana, "", 8);
                        FormatarRange(cellDesc1, dados.DiaDaSemana, "hh:mm", 60);
                    }

                    Range cellEntrada = (Range)ExcelWorksheet.get_Range("D" + indice, "D" + indice);
                    Range cellSaida = (Range)ExcelWorksheet.get_Range("E" + indice, "E" + indice);
                    var Soma = ExcelWorksheet.get_Range("F" + indice, "F" + indice);
                    Range cellProjeto = (Range)ExcelWorksheet.get_Range("G" + indice);
                    Range cellDesc = (Range)ExcelWorksheet.get_Range("H" + indice);

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

            CriarLinhaTotalizadora(indiceFinal + 1, totalizadorInicio);
        }

        /// <summary>
        /// Exporta para um modelo exitente
        /// </summary>
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

                foreach (Worksheet worksheet in work.Worksheets)
                    ExcelWorksheet = worksheet;

                if (ExcelWorksheet == null)
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
                            linhaEditavel = ObterLinhaDaData(Convert.ToDateTime(dados[0]));
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

                                Range cellEntrada = (Range)ExcelWorksheet.get_Range("D" + linhaEditavel, "D" + linhaEditavel);
                                Range cellSaida = (Range)ExcelWorksheet.get_Range("E" + linhaEditavel, "E" + linhaEditavel);
                                Range cellDesc = cellDesc = (Range)ExcelWorksheet.get_Range("H" + linhaEditavel, "H" + linhaEditavel);

                                cellEntrada.Value = entrada.ToShortTimeString();
                                cellSaida.Value = saida.ToShortTimeString();
                                cellDesc.Value = desc;

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
        public static int ObterLinhaDaData(DateTime data)
        {
            var dataLinha = new DateTime();
            int startIndex = 0;
            while (data.ToShortDateString() != dataLinha.ToShortDateString())
            {
                startIndex++;

                Range cellRotulo = (Range)ExcelWorksheet.get_Range("A" + startIndex, "A" + startIndex);

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
        ///Cria cabecalho onde carrega as horas 
        /// </summary>
        private static void CriarCabecalhoDoArquivo()
        {
            Range range = ObterRange("A1", "H2");
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("FFFFFF", 16)));
            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("366092", 16)));
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.VerticalAlignment = XlVAlign.xlVAlignTop;

            range = ObterRange("A1", "H1");
            range.RowHeight = 80;

            range = ObterRange("A2", "H2");
            range.RowHeight = 45;

            range = ObterRange("A1", "B2");
            range.Merge();
            range.ColumnWidth = 20;
            range.Font.Size = 18;
            range.Value = "TIME SHEET";
            range.Orientation = "90";
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range = ObterRange("C1", "C2");
            range.Merge();
            range.ColumnWidth = 10;
            range.Font.Size = 18;
            range.NumberFormat = "dd/mmm";
            range.Value = "'ABRIL 2014";
            range.Orientation = 90;
            range.HorizontalAlignment = XlHAlign.xlHAlignLeft;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range = ObterRange("D1", "H1");
            range.Merge();
            range.Font.Size = 20;
            range.Value = "Profissional: NOME";
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range = ObterRange("D2");
            range.Merge();
            range.Font.Size = 18;
            range.NumberFormat = "hh:mm";
            range.Formula = "= SUM(F5:F150)";
            range.HorizontalAlignment = XlHAlign.xlHAlignCenter;
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range = ObterRange("E2");
            range.Font.Size = 16;
            range.Value = "Total horas mês";
            range.VerticalAlignment = XlVAlign.xlVAlignCenter;

            range = ObterRange("A3", "H3");
            range.RowHeight = 7;
        
        }

        /// <summary>
        /// Cria o cabecalho da tabela
        /// </summary>
        private static void CriarCabecalhoTabela()
        {
            Range range = ObterRange("A4", "H4");
            range.RowHeight = 30;
            range.Font.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("FFFFFF", 16)));
            range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("24405C", 16)));

            range = ObterRange("A4");
            range.Value = "Data";

            range = ObterRange("B4");
            range.Value = "Dia da semana";
            range.WrapText = true;

            range = ObterRange("C4");
            range.Value = "Total Dia";

            range = ObterRange("D4");
            range.Value = "Inicio";

            range = ObterRange("E4");
            range.Value = "Fim";

            range = ObterRange("F4");
            range.Value = "Total Ativ";
            range.WrapText = true;

            range = ObterRange("G4");
            range.Value = "Projeto";

            range = ObterRange("H4");
            range.Value = "Descrição das atividades";
        }

        /// <summary>
        /// Cria a linha totalizadora no final de cada semana
        /// </summary>
        /// <param name="indice"></param>
        /// <param name="excelWorksheet"></param>
        private static void CriarLinhaTotalizadora(int indice, int inicio)
        {
            var rengeTotalDaSemana = ExcelWorksheet.get_Range("A" + indice, "D" + indice);
            var rengeValor = ExcelWorksheet.get_Range("E" + indice, "E" + indice);
            var Renge1 = ExcelWorksheet.get_Range("F" + indice, "F" + indice);
            var Renge2 = ExcelWorksheet.get_Range("G" + indice, "G" + indice);
            var Renge3 = ExcelWorksheet.get_Range("H" + indice, "H" + indice);

            rengeTotalDaSemana.Merge();
            rengeValor.Merge();
            FormatarRange(rengeTotalDaSemana, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(rengeValor, Registro.Semana.Segunda, "hh:mm", 0, true);
            FormatarRange(Renge1, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(Renge2, Registro.Semana.Segunda, "", 0, true);
            FormatarRange(Renge3, Registro.Semana.Segunda, "", 0, true);

            rengeTotalDaSemana.HorizontalAlignment = XlHAlign.xlHAlignRight;
            rengeTotalDaSemana.Value = "Total da semana:";
            rengeValor.Value = string.Format("= SUM(F{0}:F{1})", inicio, indice -1);
        }

        /// <summary>
        /// Formata um range
        /// </summary>
        /// <param name="range"></param>
        /// <param name="formato"></param>
        private static void FormatarRange(Range range, Registro.Semana diaDaSemana, string formato = null, int width = 0, bool isTotalizador = false)
        {

            if (diaDaSemana == Registro.Semana.Domingo
               || diaDaSemana == Registro.Semana.Sabado)
            {
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Silver);
            }

            FormatarRange(range, formato, width, isTotalizador);

        }

        /// <summary>
        /// Estiliza uma celula independente do dia da semana
        /// </summary>
        /// <param name="range"></param>
        /// <param name="formato"></param>
        private static void FormatarRange(Range range, string formato = null, int width = 0, bool isTotalizador = false)
        {
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeBottom].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeTop].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeLeft].Color = Color.Black.ToArgb();
            range.Borders[Microsoft.Office.Interop.Excel.XlBordersIndex.xlEdgeRight].Color = Color.Black.ToArgb();

            if (!string.IsNullOrEmpty(formato))
                range.NumberFormat = formato;

            if (width > 0)
                range.ColumnWidth = width;

            if (isTotalizador)
            {
                range.Font.Bold = true;
                range.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.FromArgb(Convert.ToInt32("8db4e2", 16)));
            }
        }

        /// <summary>
        /// Obtém uma celula
        /// </summary>
        /// <param name="indiceInicial"></param>
        /// <param name="indiceFinal"></param>
        /// <returns></returns>
        private static Range ObterRange(string indiceInicial, string indiceFinal = null)
        {
            if (indiceFinal == null)
                return ExcelWorksheet.get_Range(indiceInicial);
            else
                return ExcelWorksheet.get_Range(indiceInicial, indiceFinal);
        }

    }
}
