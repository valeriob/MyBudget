using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExcelImporter
{
    public class Movimento
    {
        public DateTime Data { get; set; }
        public string Categoria { get; set; }
        public string Descrizione { get; set; }
        public decimal Spesa { get; set; }

        public string DistributionKey { get; set; }

        public CreateLine ToCreateLine(BudgetId budgetId, string userId, IEnumerable<MyBudget.Projections.Category> categories)
        {
            var category = Categoria.Trim().Replace((char)160, ' ');
            var categoryId = categories.FirstOrDefault(d => string.Compare(d.Name, category, true) == 0).Id;
            var expense = new Expense(new Amount(Currencies.Euro(), Convert.ToDecimal(Spesa)), Data, categoryId, Descrizione, DistributionKey);

            return new CreateLine
            {
                Id = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                BudgetId = budgetId.ToString(),
                LineId = LineId.Create(budgetId).ToString(),
                UserId = userId,
                Expense = expense,
                
            };
        }

        public override string ToString()
        {
            return string.Format("{3} il {0:d} : {1} - {2}", Data, Categoria, Descrizione, Spesa);
        }
    }
}
