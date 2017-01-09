using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolidaExcel
{
    class Genera
    {
        public void Run(string fileSpese, string fileAnalisi)
        {
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
                }


                analisiWb.SaveAs(fileAnalisi);

            }
        }
    }
}
