using ClosedXML.Excel;
using System;
using System.Linq;

namespace ConsolidaSpeseComuni
{
    public class Movimento
    {
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
        public string Per { get; set; }
        public string Descrizione { get; set; }
        public decimal Spesa { get; set; }

        public string DistributionKey { get; set; }


        public static Movimento TryParse2017(IXLRangeRow row)
        {
            try
            {
                var data = (DateTime)row.Cell(1).Value;
                var categoria = (string)row.Cell(2).Value;
                var per = (string)row.Cell(3).Value;
                var descrizione = (string)row.Cell(4).Value;
                var spesa = Convert.ToDecimal(row.Cell(5).Value);

                return new Movimento
                {
                    Data = data,
                    Categoria = categoria,
                    Per = per,
                    Descrizione = descrizione,
                    Spesa = spesa,
                };
            }
            catch
            {
                return null;
            }
        }

        public static Movimento TryParse2013(IXLRangeRow row)
        {
            try
            {
                var dataValue = row.Cell(1).Value;
                var categoriaValue = row.Cell(2).Value;
                var descrizioneValue = row.Cell(3).Value;
                var spesaValue = row.Cell(4).Value;

                if (Vuoto(dataValue, spesaValue))
                {
                    return null;
                }

                DateTime data = DateTime.MinValue;
                if (dataValue is DateTime)
                    data = (DateTime)dataValue;
                if (dataValue is string)
                    data = DateTime.Parse((string)dataValue);

                var categoria = (string)categoriaValue;
                var descrizione = (string)descrizioneValue;
                var spesa = Convert.ToDecimal(spesaValue);

                return new Movimento
                {
                    Data = data,
                    Categoria = categoria,
                    Descrizione = descrizione,
                    Spesa = spesa,
                };
            }
            catch
            {
                throw;
            }
        }

        static string[] skipDataValues = new[] { "Data", "Laura", "Valerio", "Comune" };
        static bool Vuoto(object dataValue, object spesaValue)
        {
            var dataVuota = dataValue == null || (dataValue is string && (string.IsNullOrEmpty(dataValue as string) || skipDataValues.Contains(dataValue as string)));
            var spesaVuota = spesaValue == null || (spesaValue is string && (string.IsNullOrEmpty(spesaValue as string)));
            return dataVuota || spesaVuota;
        }

        public override string ToString()
        {
            return string.Format("{3} il {0:d} : {1} - {2}", Data, Categoria, Descrizione, Spesa);
        }
    }


}
