using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyBudget.Infrastructure;
using MyBudget.Domain.Budgets;
using MyBudget.Domain.Accounts;
using MyBudget.Domain.Users;
using MyBudget.Domain.ValueObjects;

namespace MyBudget.Tests
{
    [TestFixture]
    public class budget_should
    {
        [Test]
        public void be_created()
        {
            var budgetId = BudgetId.Create();
            var ownerId = UserId.CreateNew();
            var budget = new Budget();

            budget.Create(budgetId, "name", ownerId, Currencies.Euro().IsoCode);

            var events = budget.GetUncommittedEvents();

            Assert.AreEqual(1, events.Count());
            Assert.IsTrue(events.OfType<BudgetCreated>().Any());
        }

    }
}
