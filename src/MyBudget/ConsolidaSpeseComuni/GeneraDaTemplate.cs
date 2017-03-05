using ClosedXML.Excel;
using LinqToExcel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsolidaSpeseComuni
{
    class GeneraDaTemplate
    {
        public void Run(string fileSpese, string fileAnalisi, string template)
        {
            var numeroDiAnniFinoAdOggi = DateTime.Today.Year - 2013;
            var anni = Enumerable.Range(2013, numeroDiAnniFinoAdOggi + 1);

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
                        var lastRowNumber = wsAnno.LastRowUsed().FirstCell().Address.RowNumber;

                        if (anno < 2017)
                        {
                            riga = Estrai_Template_2013(wsAnno, lastRowNumber, dati, riga);
                        }
                        if (anno >= 2017)
                        {
                            riga = Estrai_Template_2017(wsAnno, lastRowNumber, dati, riga);
                        }
                    }

                    var dataAsTable = dati.RangeUsed().AsTable();
                    dati.Tables.Add(dataAsTable);
                }
                analisiWb.SaveAs(fileAnalisi);
            }

        }

        int Estrai_Template_2013(IXLWorksheet wsAnno, int lastRowNumber, IXLWorksheet dati, int riga)
        {
            var laura = wsAnno.Range("B1", "E" + lastRowNumber).Rows().Select(Movimento.TryParse2013);
            var valerio = wsAnno.Range("G1", "J" + lastRowNumber).Rows().Select(Movimento.TryParse2013);
            var comune = wsAnno.Range("L1", "O" + lastRowNumber).Rows().Select(Movimento.TryParse2013);

            var movimenti = laura.Concat(valerio).Concat(comune).Where(r => r != null).ToArray();

            foreach (var movimento in movimenti)
            {
                dati.Cell(riga, "A").Value = movimento.Data;
                dati.Cell(riga, "B").Value = movimento.Categoria;
                dati.Cell(riga, "C").Value = movimento.Per;
                dati.Cell(riga, "D").Value = movimento.Descrizione;
                dati.Cell(riga, "E").Value = movimento.Spesa;
                riga++;
            }
            return riga;
        }

        int Estrai_Template_2017(IXLWorksheet wsAnno, int lastRowNumber, IXLWorksheet dati, int riga)
        {
            var laura = wsAnno.Range("B1", "F" + lastRowNumber).Rows().Select(Movimento.TryParse2017);
            var valerio = wsAnno.Range("H1", "L" + lastRowNumber).Rows().Select(Movimento.TryParse2017);
            var comune = wsAnno.Range("N1", "R" + lastRowNumber).Rows().Select(Movimento.TryParse2017);

            var movimenti = laura.Concat(valerio).Concat(comune).Where(r => r != null).ToArray();

            foreach (var movimento in movimenti)
            {
                dati.Cell(riga, "A").Value = movimento.Data;
                dati.Cell(riga, "B").Value = movimento.Categoria;
                dati.Cell(riga, "C").Value = movimento.Per;
                dati.Cell(riga, "D").Value = movimento.Descrizione;
                dati.Cell(riga, "E").Value = movimento.Spesa;
                riga++;
            }
            return riga;
        }


    }
}
