using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DividendiAggregati
{
    class Program
    {
        private const string AggregatoFileName = "DividendiAggregati.xlsx";

        static async Task Main(string[] args)
        {
            //Debugger.Launch();
            //Debugger.Break();

            var binckFolder = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Bink";
            binckFolder = args[0];

            var binck = new DividendiBinck.ElaboraDividendiBinck();
            var binckOutput = await binck.ElaboraDividendi(binckFolder);
            var binckDividendi = CaricaDividendi(binckOutput, "Binck");

            var finecoFolder = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Fineco";
            finecoFolder = args[1];
            var fineco = new DividendiFineco.ElaboraDividendiFineco();
            var finecoOutput = await fineco.ElaboraDividendi(finecoFolder);
            var finecoDividendi = CaricaDividendi(finecoOutput, "Fineco");

            var ispFile = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Intesa Sanpaolo\Dividendi Intesa San Paolo.xlsx";
            ispFile = args[2];
            var ispOutput = CaricaDividendi(ispFile, "ISP");

            var outputFile = AggregatoFileName;
            var templateFile = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi\Template.xlsx";
            if (File.Exists(outputFile))
                File.Delete(outputFile);
            File.Copy(templateFile, outputFile);
            var fullDebug = Path.GetFullPath(outputFile);

            AggregaDividendi(binckDividendi, finecoDividendi, ispOutput, outputFile);
        }

        static void AggregaDividendi(Dividendo[] binckDividendi, Dividendo[] finecoDividendi, Dividendo[] ispDividendi, string outputFile)
        {
            //System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("it-IT");
            //tmp test
            //binckDividendi = Array.Empty<Dividendo>();
            //finecoDividendi = Array.Empty<Dividendo>();

            var dividendi = binckDividendi.Concat(finecoDividendi).Concat(ispDividendi).OrderBy(r => r.Data).ToArray();

            using (var sourceFile = new XLWorkbook(outputFile))
            {
                if (sourceFile.TryGetWorksheet("Dividendi", out IXLWorksheet sheetTotaleDividendi) == false)
                {
                    sheetTotaleDividendi = sourceFile.AddWorksheet("Dividendi");
                    AddHeaderDividendiToSheet(sheetTotaleDividendi);
                }

                var rowIndexDividendi = 2;
                foreach (var importo in dividendi)
                {
                    AppendRowDividendiToSheet(sheetTotaleDividendi, rowIndexDividendi, importo);
                    rowIndexDividendi++;
                }

                sourceFile.Save();
            }

        }


        static void AddHeaderDividendiToSheet(IXLWorksheet wb)
        {
            var header = wb.Row(1);
            header.Cell("A").Value = "Simbolo";
            header.Cell("B").Value = "Descrizione";
            header.Cell("C").Value = "Data";
            header.Cell("D").Value = "Importo";
            header.Cell("E").Value = "Conto";
        }
        static void AppendRowDividendiToSheet(IXLWorksheet sheet, int rowIndex, Dividendo importo)
        {
            var row = sheet.Row(rowIndex);
            row.Cell("A").Value = importo.Simbolo;
            row.Cell("B").Value = importo.Descrizione;
            row.Cell("C").Value = importo.Data;
            row.Cell("D").Value = importo.Importo;
            row.Cell("E").Value = importo.Conto;
        }

        static Dividendo[] CaricaDividendi(string file, string conto)
        {
            var result = new List<Dividendo>();

            using (var sourceFile = new XLWorkbook(file))
            {
                foreach (var ws in sourceFile.Worksheets.Where(w => w.Name == "Dividendi"))
                {
                    var lastRow = ws.LastRowUsed();
                    var importi = ws.Rows(2, lastRow.RowNumber()).Select(d => Dividendo.TryParse(d, conto)).ToArray();

                    foreach (var importo in importi)
                    {
                        if (importo.IsValid())
                            result.Add(importo);
                    }
                }
            }

            return result.ToArray();
        }


    }




    public class Dividendo
    {
        public string Simbolo { get; set; }
        public string Descrizione { get; set; }
        public DateTime Data { get; set; }
        public decimal Importo { get; set; }
        public string Conto { get; set; }

        public bool IsValid()
        {
            return string.IsNullOrEmpty(Simbolo) == false && Importo != 0;
        }

        public static Dividendo TryParse(IXLRow row, string conto)
        {
            var simboloValue = row.Cell("A").Value;
            var descrizione = row.Cell("B").Value;
            var dataValue = row.Cell("C").Value;
            var data = ParseData(dataValue);

            var importoCell = row.Cell("D");
            var importoValue = importoCell.Value;

            decimal.TryParse(importoValue.ToString(), out decimal importo);

            return new Dividendo
            {
                Simbolo = simboloValue.ToString(),
                Descrizione = descrizione.ToString(),
                Data = data,
                Importo = importo,
                Conto = conto,
            };
        }

        static DateTime ParseData(object dataValue)
        {
            DateTime data = DateTime.MinValue;
            if (dataValue is DateTime)
                data = (DateTime)dataValue;
            if (dataValue is string)
            {
                if (string.IsNullOrEmpty(dataValue as string))
                    return DateTime.MinValue;
                else
                    data = DateTime.Parse((string)dataValue);
            }
            return data;
        }

        public override string ToString()
        {
            return $"{Simbolo} {Data.Date.ToShortDateString()} {Importo:0.00}";
        }
    }



}
