using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolidaExcel
{
    class GeneraDaTemplate
    {
        public void Run(string fileSpese, string fileAnalisi, string template)
        {
            var numeroDiAnniFinoAdOggi = DateTime.Today.Year - 2011;
            var anni = Enumerable.Range(2011, numeroDiAnniFinoAdOggi + 1);

            File.Copy(template, fileAnalisi, true);

            using (var analisiWb = new XLWorkbook(fileAnalisi))
            {
                var dati = analisiWb.Worksheet("Dati");
                int riga = 2;

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
                            dati.Cell(riga, "E").Value = movimento.Tag;
                            riga++;
                        }

                    }

                    var dataAsTable = dati.RangeUsed().AsTable();
                    dati.Tables.Add(dataAsTable);


                    //var range = dati.Range("B1", "B30");

                    //var pivotSh = analisiWb.Worksheet("Pivot");
                    //pivotSh.PivotTable("PivotTable1").SetRefreshDataOnOpen(true);

                }


                analisiWb.Save();

            }
        }
    }
}
