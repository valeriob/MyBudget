using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolidaExcel
{
    class Program
    {
        static void Main(string[] args)
        {
            var fileSpese = args[0];
            var fileAnalisi = args[1];
            //var file = @"spese.xlsx";
            //var excel = new ExcelQueryFactory(file);
            var numeroDiAnniFinoAdOggi = DateTime.Today.Year - 2011;
            var anni = Enumerable.Range(2011, numeroDiAnniFinoAdOggi + 1);

            using (var analisiWb = new XLWorkbook())
            {
                var dati = analisiWb.Worksheets.Add("Dati");
                int riga = 1;
                dati.Cell(riga, "A").Value = "Data";
                dati.Cell(riga, "B").Value = "Categoria";
                dati.Cell(riga, "C").Value = "Descrizione";
                dati.Cell(riga, "D").Value = "Spesa";
                riga++;

                using (var spese = new XLWorkbook(fileSpese))
                {
                    foreach (var anno in anni)
                    {
                        var wsAnno = spese.Worksheet(anno + "");
                        var movimenti = wsAnno.Rows().Select(Movimento.TryParse).Where(r => r != null).ToArray();

                        foreach (var movimento in movimenti)
                        {
                            dati.Cell(riga, "A").Value = movimento.Data;
                            dati.Cell(riga, "B").Value = movimento.Categoria;
                            dati.Cell(riga, "C").Value = movimento.Descrizione;
                            dati.Cell(riga, "D").Value = movimento.Spesa;
                            riga++;
                        }

                    }

                    var dataAsTable = dati.RangeUsed().AsTable();
                    dati.Tables.Add(dataAsTable);
                  

                    var range = dati.Range("B1", "B30");
                    //range = worksheet.RangeUsed();

                    var pivotSh = analisiWb.Worksheets.Add("Pivot");
                    var pivot = pivotSh.CreatePivotTable(pivotSh.Cell("F2"));
                    pivot.SourceRange = range;
        
                    ////var pivot = worksheet.PivotTables.AddNew("PivotTable", worksheet.Cell("F2"), range);
                    pivot.RowLabels.Add("Categoria");
                   // pivot.RowLabels.Add("Descrizione");
                   // pivot.ColumnLabels.Add("Data");
                   // pivot.Values.Add("Spesa");
                  

                }


                analisiWb.SaveAs(fileAnalisi);

            }


            //using (var workbook = new XLWorkbook())
            //{
            //    var worksheet = workbook.Worksheets.Add("Analisi");
            //    int riga = 1;
            //    worksheet.Cell(riga, "A").Value = "Data";
            //    worksheet.Cell(riga, "B").Value = "Categoria";
            //    worksheet.Cell(riga, "C").Value = "Descrizione";
            //    worksheet.Cell(riga, "D").Value = "Spesa";
            //    riga++;

            //    foreach (var anno in anni)
            //    {
            //        var movimenti = excel.Worksheet<Movimento>(anno + "").Where(r => r.Data != DateTime.MinValue);

            //        foreach (var movimento in movimenti)
            //        {
            //            worksheet.Cell(riga, "A").Value = movimento.Data;
            //            worksheet.Cell(riga, "B").Value = movimento.Categoria;
            //            worksheet.Cell(riga, "C").Value = movimento.Descrizione;
            //            worksheet.Cell(riga, "D").Value = movimento.Spesa;
            //            riga++;
            //        }

            //    }


            //    workbook.SaveAs("Spese Analisi.xlsx");

            //}

        }
    }
}
