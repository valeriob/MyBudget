using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBudget.Infrastructure;
using MyBudget.Budgets;

namespace MyBudget.Tests
{
    [TestFixture]
    public class budget_should
    {
        [Test]
        public void be_created()
        {
            var budgetId = new BudgetId();
            var ownerId = new AccountId();
            var budget = new Budgets.Budget();

            budget.Create(budgetId, "name", ownerId);

            var events = budget.GetUncommittedEvents();

            Assert.AreEqual(1, events.Count());
            Assert.IsTrue(events.OfType<BudgetCreated>().Any());
        }

    }
}
