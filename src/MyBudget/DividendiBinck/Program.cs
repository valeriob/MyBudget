using ClosedXML.Excel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace DividendiBinck
{
    class Program
    {
        private const string SimboliFileName = "SimboliDescrizioni.xlsx";
        private const string AggregatoFileName = "BinkAggregato.xlsx";

        static async Task Main(string[] args)
        {
            var httpClient = new HttpClient();

            var folder = @"C:\Users\Valerio\Downloads\binck\";

            folder = @"E:\Skydrive\Documents\Finanze\Investimenti\Dividendi Bink\";
            var tuttiImporti = new HashSet<BinckRow>();
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
                        var importi = ws.Rows(4, lastRow.RowNumber()).Select(r => BinckRow.TryParse(r)).ToArray();

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

            var dividendi = tuttiImporti.Where(r => r.Tipologia == "Pagamento dividendi").ToArray();

            await CSRakowski.Parallel.ParallelAsync.ForEachAsync(tuttiImporti.GroupBy(r => r.Data), async importi =>
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
                foreach (var importiAnno in tuttiImporti.GroupBy(r => r.Data.Year))
                {
                    var sheetAnno = sourceFile.AddWorksheet(importiAnno.Key.ToString());

                    AddHeaderToSheet(sheetAnno);

                    var rowIndexAnno = 2;
                    foreach (var importo in importiAnno)
                    {
                        AppendRowToSheet(sheetAnno, rowIndexAnno, importo);
                        rowIndexAnno++;
                    }
                }

                var sheetTotale = sourceFile.AddWorksheet("Totale");
                AddHeaderToSheet(sheetTotale);

                var rowIndex = 2;
                foreach (var importo in tuttiImporti.OrderBy(r => r.Data))
                {
                    AppendRowToSheet(sheetTotale, rowIndex, importo);
                    rowIndex++;
                }

                sourceFile.SaveAs(Path.Combine(folder, AggregatoFileName));
            }


        }

        static void AppendRowToSheet(IXLWorksheet sheet, int rowIndex, BinckRow importo)
        {
            var row = sheet.Row(rowIndex);
            row.Cell("A").Value = importo.Simbolo;
            row.Cell("B").Value = "";
            row.Cell("C").Value = importo.Data;
            row.Cell("D").Value = Math.Round(importo.ImportoEuro, 2);
        }

        static void AddHeaderToSheet(IXLWorksheet wb)
        {
            var header = wb.Row(1);
            header.Cell("A").Value = "Simbolo";
            header.Cell("B").Value = "Descrizione";
            header.Cell("C").Value = "Data";
            header.Cell("D").Value = "Importo";
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

    public class BinckRow : IEquatable<BinckRow>
    {
        public DateTime Data { get; set; }
        public DateTime DataValuta { get; set; }
        public string Tipologia { get; set; }
        public string Descrizione { get; set; }
        public decimal Importo { get; set; }
        public string ImportoValuta { get; set; }

        public decimal Cambio_EUR_Valuta { get; set; }
        public decimal ImportoEuro { get; set; }
        public string Simbolo { get; set; }

        public void ConvertiInEuro(Rates rates)
        {
            switch (ImportoValuta)
            {
                case "$":
                    Cambio_EUR_Valuta = (decimal)rates.USD;
                    ImportoEuro = Importo / Cambio_EUR_Valuta;
                    break;
                case "€":
                    ImportoEuro = Importo;
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


        public static BinckRow TryParse(IXLRow row)
        {
            var dataValue = row.Cell("B").Value;
            var dataValutaValue = row.Cell("C").Value;
            var tipologia = row.Cell("D").Value;
            var descrizione = row.Cell("E").Value;

            var cellF = row.Cell("F");
            var importoValue = cellF.Value;


            var data = ParseData(dataValue);
            var dataValuta = ParseData(dataValutaValue);

            var valuta = "";

            if (cellF.Style.NumberFormat.Format.Contains("[$$-409]"))
                valuta = "$";
            if (cellF.Style.NumberFormat.Format.Contains("[$€-410]"))
                valuta = "€";

            var importoValueString = importoValue.ToString();
            decimal importo = 0;
            if (importoValueString != "-")
            {
                importo = Convert.ToDecimal(importoValue.ToString());
            }

            return new BinckRow
            {
                Data = data,
                DataValuta = dataValuta,
                Tipologia = tipologia.ToString(),
                Descrizione = descrizione.ToString(),
                Importo = importo,
                ImportoValuta = valuta,
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

        public bool Equals(BinckRow other)
        {
            return other != null && Data.Equals(other.Data) && Tipologia == other.Tipologia && Descrizione == other.Descrizione && Importo == other.Importo && ImportoValuta == other.ImportoValuta;
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as BinckRow);
        }
        public override int GetHashCode()
        {
            return 3 * Data.GetHashCode() + 5 * Tipologia.GetHashCode() + 7 * Descrizione.GetHashCode() + 11 * Importo.GetHashCode() + 13 * ImportoValuta.GetHashCode();
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
