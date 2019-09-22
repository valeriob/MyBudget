using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DividendiFineco
{
    class Program
    {
        private const string SimboliFileName = "SimboliDescrizioni.xlsx";
        private const string AggregatoFileName = "FinecoAggregato.xlsx";

        static async Task Main(string[] args)
        {
            System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("it-IT");
            //Debugger.Launch();

            var httpClient = new HttpClient();

            //var folder = @"C:\Users\Valerio\Downloads\binck\";
            var folder = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Fineco\";
            //var folder = "";
            if (args.Length > 0)
            {
                folder = args[0];
            }


            var tuttiImporti = new HashSet<FinecoRow>();
            var titoli = CaricaTitoli(folder);

            foreach (var file in Directory.EnumerateFiles(folder, "*.xlsx", SearchOption.AllDirectories))
            {
                if (Path.GetExtension(file).ToUpper() != ".XLSX")
                    continue;
                if (Path.GetFileName(file).Equals(SimboliFileName))
                    continue;
                if (Path.GetFileName(file).Equals(AggregatoFileName))
                    continue;

                using (var sourceFile = new XLWorkbook(file))
                {
                    foreach (var ws in sourceFile.Worksheets)
                    {
                        var lastRow = ws.LastRowUsed();
                        var importi = ws.Rows(7, lastRow.RowNumber()).Select(FinecoRow.TryParse).ToArray();

                        foreach (var importo in importi)
                        {
                            //var rates = await GetCambioEUR_USDInData(httpClient, importo.DataValuta);
                            //importo.ConvertiInEuro(rates);
                            //importo.CaricaSimbolo(titoli);

                            tuttiImporti.Add(importo);
                        }
                    }
                }
            }

            var dividendi = CalcolaDividendiNetti(tuttiImporti.ToArray());

            await CSRakowski.Parallel.ParallelAsync.ForEachAsync(dividendi.GroupBy(r => r.Data), async importi =>
            {
                var rates = await GetCambioEUR_USDInData(httpClient, importi.Key);

                foreach (var importo in importi)
                {
                    importo.ConvertiInEuro(rates);
                    importo.CaricaSimbolo(titoli);
                }
            });

            using (var sourceFile = new XLWorkbook())
            {
                foreach (var importiAnno in dividendi.GroupBy(r => r.Data.Year))
                {
                    var sheetAnno = sourceFile.AddWorksheet(importiAnno.Key.ToString());

                    AddHeaderDividendiToSheet(sheetAnno);

                    var rowIndexAnno = 2;
                    foreach (var importo in importiAnno)
                    {
                        AppendRowDividendiToSheet(sheetAnno, rowIndexAnno, importo);
                        rowIndexAnno++;
                    }
                }

                var sheetTotaleDividendi = sourceFile.AddWorksheet("Totale Dividendi");
                AddHeaderDividendiToSheet(sheetTotaleDividendi);

                var rowIndexDividendi = 2;
                foreach (var importo in dividendi.OrderBy(r => r.Data))
                {
                    AppendRowDividendiToSheet(sheetTotaleDividendi, rowIndexDividendi, importo);
                    rowIndexDividendi++;
                }


                var sheetTutto = sourceFile.AddWorksheet("Tutto");
                AddHeaderTotaleToSheet(sheetTutto);

                var rowIndexTotale = 2;
                foreach (var importo in tuttiImporti.OrderBy(r => r.Data))
                {
                    AppendRowTotaleToSheet(sheetTutto, rowIndexTotale, importo);
                    rowIndexTotale++;
                }

                sourceFile.SaveAs(Path.Combine(folder, AggregatoFileName));
            }


        }

        static Dividendo[] CalcolaDividendiNetti(FinecoRow[] importi)
        {
            return importi.GroupBy(r => new { r.Data, Descrizione = r.EstraiDescrizioneTitolo() })
                .Where(r => string.IsNullOrEmpty(r.Key.Descrizione) == false)
                .Select(r => new Dividendo
                {
                    Data = r.Key.Data,
                    Descrizione = r.Key.Descrizione,
                    Importo = r.Sum(t => t.Entrate) - r.Sum(t => t.Uscite),
                    ImportoValuta = r.Select(s=> s.ImportoValuta).FirstOrDefault(),
                }).ToArray();
        }

        static void AddHeaderDividendiToSheet(IXLWorksheet wb)
        {
            var header = wb.Row(1);
            header.Cell("A").Value = "Simbolo";
            header.Cell("B").Value = "Descrizione";
            header.Cell("C").Value = "Data";
            header.Cell("D").Value = "Importo";
            header.Cell("E").Value = "Importo USD";
            header.Cell("F").Value = "Cambio EUR/USD";
        }

        static void AppendRowDividendiToSheet(IXLWorksheet sheet, int rowIndex, Dividendo importo)
        {
            var row = sheet.Row(rowIndex);
            row.Cell("A").Value = importo.Simbolo;
            row.Cell("B").Value = importo.Descrizione;
            row.Cell("C").Value = importo.Data;
            row.Cell("D").Value = Math.Round(importo.ImportoEuro, 2);
            row.Cell("E").Value = Math.Round(importo.Importo, 2);
            row.Cell("F").Value = Math.Round(importo.Cambio_EUR_Valuta, 2);
        }

        static void AddHeaderTotaleToSheet(IXLWorksheet wb)
        {
            var header = wb.Row(1);
            header.Cell("A").Value = "Numero";
            header.Cell("B").Value = "Tipologia";

            header.Cell("C").Value = "Simbolo";
            header.Cell("D").Value = "Descrizione";
            header.Cell("E").Value = "Data";
            header.Cell("F").Value = "Importo";
            header.Cell("G").Value = "Entrate USD";
            header.Cell("H").Value = "Uscite USD";

        }


        static void AppendRowTotaleToSheet(IXLWorksheet sheet, int rowIndex, FinecoRow importo)
        {
            var row = sheet.Row(rowIndex);

            row.Cell("B").Value = importo.Causale;

            row.Cell("C").Value = importo.Simbolo;
            row.Cell("D").Value = importo.Descrizione;
            row.Cell("E").Value = importo.Data;
            row.Cell("F").Value = Math.Round(importo.ImportoEuro, 2);
            row.Cell("G").Value = Math.Round(importo.Entrate, 2);
            row.Cell("H").Value = Math.Round(importo.Uscite, 2);
        }




        public static async Task<Rates> GetCambioEUR_USDInData(HttpClient client, DateTime data)
        {
            // https://exchangeratesapi.io/
            var url = $@"https://api.exchangeratesapi.io/{data.Year}-{data.Month}-{data.Day}?symbols=USD";
            var response = await client.GetAsync(url);
            var json = await response.Content.ReadAsStringAsync();
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<Rootobject>(json);
            return result.rates;
        }

        public static Titolo[] CaricaTitoli(string folder)
        {
            using (var sourceFile = new XLWorkbook(Path.Combine(folder, SimboliFileName)))
            {
                foreach (var wb in sourceFile.Worksheets)
                {
                    return wb.Rows(2, 100).Select(Titolo.Parse).Where(r => r.Valido()).ToArray();
                }
            }
            return Array.Empty<Titolo>();
        }
    }

    public class Titolo
    {
        public string Simbolo { get; set; }
        public string Descrizione { get; set; }

        public bool Valido()
        {
            return string.IsNullOrEmpty(Descrizione) == false;
        }

        public static Titolo Parse(IXLRow row)
        {
            return new Titolo
            {
                Simbolo = row.Cell("A").Value.ToString(),
                Descrizione = row.Cell("B").Value.ToString()
            };
        }
    }

    public class FinecoRow : IEquatable<FinecoRow>
    {
        public DateTime Data { get; set; }
        public DateTime DataValuta { get; set; }
        public string Causale { get; set; }
        public string Descrizione { get; set; }
        public decimal Entrate { get; set; }
        public decimal Uscite { get; set; }

        public string ImportoValuta { get; set; }
        public decimal SaldoAttuale { get; set; }

        public decimal Cambio_EUR_Valuta { get; set; }
        public decimal ImportoEuro { get; set; }
        public string Simbolo { get; set; }


        public string EstraiDescrizioneTitolo()
        {
            /*  Div.su 370,000 PIMCO DYNAMIC CREDIT
                Rit.div.su 370,000 PIMCO DYNAMIC CREDIT
            */

            var result = "";
            var div = Descrizione.StartsWith("Div.su");
            var rit = Descrizione.StartsWith("Rit.div.su");
            if (div)
                result = Descrizione.Remove(0, 7);
            if (rit)
                result = Descrizione.Remove(0, 11);

            return result;
        }

        public void ConvertiInEuro(Rates rates)
        {
            switch (ImportoValuta)
            {
                case "$":
                    Cambio_EUR_Valuta = (decimal)rates.USD;
                    ImportoEuro = Entrate / Cambio_EUR_Valuta;
                    break;
                case "€":
                    ImportoEuro = Entrate;
                    break;
                default:
                    break;
            }

        }

        public void CaricaSimbolo(Titolo[] titoli)
        {
            Simbolo = titoli.Where(r => Descrizione.Contains(r.Descrizione)).Select(s => s.Simbolo).FirstOrDefault();
        }

        public OutoutRow ToOutoutRow()
        {
            return new OutoutRow
            {
                Simbolo = Simbolo,
                Data = Data,
                Importo = ImportoEuro,
            };
        }


        public static FinecoRow TryParse(IXLRow row)
        {
            var dataValue = row.Cell("A").Value;
            var data = ParseData(dataValue);
            var dataValutaValue = row.Cell("B").Value;
            var dataValuta = ParseData(dataValutaValue);

            var entrateValue = row.Cell("C").Value + "";
            decimal entrate = 0;
            if (string.IsNullOrEmpty(entrateValue) == false)
                entrate = Convert.ToDecimal(entrateValue);

            var usciteValue = row.Cell("D").Value + "";
            decimal uscite = 0;
            if (string.IsNullOrEmpty(usciteValue) == false)
                uscite = Convert.ToDecimal(usciteValue);

            var causale = row.Cell("E").Value;
            var descrizione = row.Cell("F").Value;

            //var cellF = row.Cell("F");
            //var importoValue = cellF.Value;

            //var saldoAttualeValue = row.Cell("G").Value;




            //var valuta = "";

            //if (cellF.Style.NumberFormat.Format.Contains("[$$-409]"))
            //    valuta = "$";
            //if (cellF.Style.NumberFormat.Format.Contains("[$€-410]"))
            //    valuta = "€";

            //var importoValueString = entrateValue.ToString();
            //decimal importo = 0;
            //if (importoValueString != "-")
            //{
            //    importo = Convert.ToDecimal(entrateValue.ToString());
            //}

            //var saldoAttuale = Convert.ToDecimal(saldoAttualeValue.ToString());

            return new FinecoRow
            {
                Data = data,
                DataValuta = dataValuta,
                Entrate = entrate,
                Uscite = uscite,
                Causale = causale.ToString(),
                Descrizione = descrizione.ToString(),
                ImportoValuta = "USD",

                //SaldoAttuale = saldoAttuale
            };
        }

        static DateTime ParseData(object dataValue)
        {
            DateTime data = DateTime.MinValue;
            if (dataValue is DateTime)
                data = (DateTime)dataValue;
            if (dataValue is string)
                data = DateTime.Parse((string)dataValue);
            return data;
        }

        public bool Equals(FinecoRow other)
        {
            return other != null && Data.Equals(other.Data) && Causale == other.Causale && Descrizione == other.Descrizione && Entrate == other.Entrate && ImportoValuta == other.ImportoValuta;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as FinecoRow);
        }
        public override int GetHashCode()
        {
            return 3 * Data.GetHashCode() + 5 * Causale.GetHashCode() + 7 * Descrizione.GetHashCode() + 11 * Entrate.GetHashCode() + 13 * ImportoValuta.GetHashCode();
        }
    }

    public class Dividendo
    {
        public DateTime Data { get; set; }
        public decimal Importo { get; set; }
        public string ImportoValuta { get; set; }

        public string Descrizione { get; set; }
        public decimal ImportoEuro { get; set; }

        public decimal Cambio_EUR_Valuta { get; set; }
        public string Simbolo { get; set; }


        public void CaricaSimbolo(Titolo[] titoli)
        {
            Simbolo = titoli.Where(r => Descrizione.Contains(r.Descrizione)).Select(s => s.Simbolo).FirstOrDefault();
        }

        public void ConvertiInEuro(Rates rates)
        {
            switch (ImportoValuta)
            {
                case "USD":
                    Cambio_EUR_Valuta = (decimal)rates.USD;
                    ImportoEuro = Importo / Cambio_EUR_Valuta;
                    break;
                case "EUR":
                    ImportoEuro = Importo;
                    break;
                default:
                    break;
            }
        }
    }

    public class OutoutRow
    {
        public string Simbolo { get; set; }
        public DateTime Data { get; set; }
        public decimal Importo { get; set; }
    }

    public class Rootobject
    {
        public Rates rates { get; set; }
        public string _base { get; set; }
        public string date { get; set; }
    }

    public class Rates
    {
        public float CAD { get; set; }
        public float HKD { get; set; }
        public float LVL { get; set; }
        public float PHP { get; set; }
        public float DKK { get; set; }
        public float HUF { get; set; }
        public float CZK { get; set; }
        public float AUD { get; set; }
        public float RON { get; set; }
        public float SEK { get; set; }
        public float IDR { get; set; }
        public float INR { get; set; }
        public float BRL { get; set; }
        public float RUB { get; set; }
        public float LTL { get; set; }
        public float JPY { get; set; }
        public float THB { get; set; }
        public float CHF { get; set; }
        public float SGD { get; set; }
        public float PLN { get; set; }
        public float BGN { get; set; }
        public float TRY { get; set; }
        public float CNY { get; set; }
        public float NOK { get; set; }
        public float NZD { get; set; }
        public float ZAR { get; set; }
        public float USD { get; set; }
        public float MXN { get; set; }
        public float EEK { get; set; }
        public float GBP { get; set; }
        public float KRW { get; set; }
        public float MYR { get; set; }
        public float HRK { get; set; }
    }

}
