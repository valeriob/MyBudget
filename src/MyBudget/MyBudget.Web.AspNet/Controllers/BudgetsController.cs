using MyBudget.Commands;
using MyBudget.Domain.Budgets;
using MyBudget.Web.AspNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class BudgetsController : MyBudgetController
    {
        public virtual ActionResult Index()
        {
            var budgets = ProjectionManager.GetBudgetsList().GetBudgetsUserCanView(GetCurrentUserId());

            var model = new BudgetListViewModel(budgets);

            return View(model);
        }

        public virtual ActionResult Details(string id)
        {
            var budgetId = new BudgetId(id);
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(budgetId);
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(budgetId);
           
            var model = new BudgetDetailsViewModel
            {
                Budget = budget,
                Categories = categories,
                DistributionKeys = budget.GetDistributionKeys(),
            };
            return View(model);
        }


        public virtual ActionResult Create()
        {
            var model = new CreateBudgetViewModel
            {
                Id = BudgetId.Create().ToString(),
                Name = "New Budget",
                Currencies = MyBudget.Domain.ValueObjects.Currencies.GetAll(),
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Create(CreateBudgetViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<CreateBudget>();
                handler(new CreateBudget
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetName = model.Name,
                    BudgetId = BudgetId.Create().ToString(),
                    CurrencyISOCode = model.CurrencyISOCode,

                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                });

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public virtual ActionResult Edit(int id)
        {
            //var model = new BudgetViewModel
            //{

            //};
            return View();
        }

        [HttpPost]
        public virtual ActionResult Edit(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add update logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }


        public virtual ActionResult Delete(int id)
        {
            return View();
        }

        [HttpPost]
        public virtual ActionResult Delete(int id, FormCollection collection)
        {
            try
            {
                // TODO: Add delete logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        [HttpPost]
        public virtual ActionResult SubmitCheckPoint(SubmitCheckPoint model)
        {
            

            return RedirectToAction(MVC.BudgetStats.ByDistribution(model.BudgetId));
            //if (ModelState.IsValid)
            //    return RedirectToAction(MVC.BudgetStats.ByDistribution(model.BudgetId));
            //else
            //    return View();
        }
    }

}
