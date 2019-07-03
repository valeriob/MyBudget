using ClosedXML.Excel;
using System;


namespace ConsolidaExcel
{
    public class Movimento
    {
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
        public string Descrizione { get; set; }
        public decimal Spesa { get; set; }
        public string Tag { get; set; }

        public string DistributionKey { get; set; }

        //public CreateLine ToCreateLine(DateTime timestamp, BudgetId budgetId, string userId, IEnumerable<MyBudget.Projections.Category> categories)
        //{
        //    var category = Categoria.Trim().Replace((char)160, ' ');
        //    var categoryId = categories.FirstOrDefault(d => string.Compare(d.Name, category, true) == 0).Id;
        //    var expense = new Expense(new Amount(Currencies.Euro(), Spesa), Data, categoryId, Descrizione, DistributionKey);

        //    return new CreateLine
        //    {
        //        Id = Guid.NewGuid(),
        //        Timestamp = timestamp,
        //        BudgetId = budgetId.ToString(),
        //        LineId = LineId.Create(budgetId).ToString(),
        //        UserId = userId,
        //        Expense = expense,
        //    };
        //}

        public static Movimento TryParse(IXLRow row)
        {
            try
            {
                var dataValue = row.Cell("A").Value;
                var categoriaValue = row.Cell("B").Value;
                var descrizioneValue = row.Cell("C").Value;
                var spesaValue = row.Cell("D").Value;
                var tagValue = row.Cell("E").Value;

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
                var tag = (string)tagValue;

                return new Movimento
                {
                    Data = data,
                    Categoria = categoria,
                    Descrizione = descrizione,
                    Spesa = spesa,
                    Tag = tag,
                };
            }
            catch
            {
                throw;
                //return null;
            }
        }

        static bool Vuoto(object dataValue, object spesaValue)
        {
            var dataVuota = dataValue == null || (dataValue is string && (string.IsNullOrEmpty(dataValue as string) || (dataValue as string).Equals("Data")));
            var spesaVuota = spesaValue == null || (spesaValue is string && (string.IsNullOrEmpty(spesaValue as string)));
            return dataVuota || spesaVuota;
        }

        public override string ToString()
        {
            return string.Format("{3} il {0:d} : {1} - {2}", Data, Categoria, Descrizione, Spesa);
        }
    }
}
