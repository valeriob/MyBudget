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
                var data = (DateTime)row.Cell("A").Value;
                var categoria = (string)row.Cell("B").Value;
                var descrizione = (string)row.Cell("C").Value;
                var spesa = Convert.ToDecimal(row.Cell("D").Value);

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
