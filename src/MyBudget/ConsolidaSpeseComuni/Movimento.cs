using ClosedXML.Excel;
using System;


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
                var data = (DateTime)row.Cell(1).Value;
                var categoria = (string)row.Cell(2).Value;
                var descrizione = (string)row.Cell(3).Value;
                var spesa = Convert.ToDecimal(row.Cell(4).Value);

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
                return null;
            }
        }

        public override string ToString()
        {
            return string.Format("{3} il {0:d} : {1} - {2}", Data, Categoria, Descrizione, Spesa);
        }
    }

 
}
