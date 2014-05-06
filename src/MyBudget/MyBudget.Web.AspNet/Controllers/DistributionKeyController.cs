using MyBudget.Commands;
using MyBudget.Web.AspNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class DistributionKeyController : MyBudgetController
    {
        public virtual ActionResult Create(string budgetId, string returnUrl)
        {
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(new MyBudget.Domain.Budgets.BudgetId(budgetId));
            var model = new DistributionKeyViewModel
            {
                BudgetId = budgetId,
                BudgetName = budget.Name,
                Name = "New DistributionKey",
                ReturnUrl = returnUrl,
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Create(DistributionKeyViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<AddBudgetDistributionKey>();
                handler(new AddBudgetDistributionKey
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetId = model.BudgetId,
                    Name = model.Name,

                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                });
                return Redirect(model.ReturnUrl);
            }
            catch
            {
                return View();
            }
        }


        public virtual ActionResult Edit(string budgetId, string categoryId)
        {
            //var bid = new Domain.Budgets.BudgetId(budgetId);
            //var category = ProjectionManager.GetCategories().GetBudgetsCategories(bid)
            //    .Single(r => r.Id == categoryId);
            //var budget = ProjectionManager.GetBudgetsList().GetBudgetById(bid);
            //var model = new DistributionKeyViewModel
            //{
            //    Id = categoryId,
            //    BudgetId = budgetId,
            //    BudgetName = budget.Name,
            //    Description = category.Description,
            //    Name = category.Name,
            //};
            //return View(model);
            return View();
        }

        [HttpPost]
        public virtual ActionResult Edit(DistributionKeyViewModel model)
        {
            //try
            //{
            //    var handler = CommandManager.Create<UpdateCategory>();
            //    handler(new UpdateCategory
            //    {
            //        UserId = GetCurrentUserId().ToString(),
            //        CategoryName = model.Name,
            //        CategoryDescription = model.Description,
            //        CategoryId = model.Id,

            //        Id = Guid.NewGuid(),
            //        Timestamp = DateTime.Now,
            //    });
            //    return RedirectToAction(Actions.Index(model.BudgetId));
            //}
            //catch
            //{
            //    return View();
            //}
            return View();
        }

    }

    public class DistributionKeyViewModel
    {
        public string BudgetId { get; set; }
        public string Name { get; set; }
        public string ReturnUrl { get; set; }

        public string BudgetName { get; set; }
    }
}