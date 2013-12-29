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
            var readModel = ProjectionManager.GetBudgetsList();
            var budgets = readModel.GetBudgetsUserCanView(GetCurrentUserId());

            var model = new BudgetListViewModel(budgets);

            return View(model);
        }

        public virtual ActionResult Details(string id)
        {
            var readModel = ProjectionManager.GetBudgetsList();
            var budget = readModel.GetBudgetById(new BudgetId(id));

            return View(budget);
        }


        public virtual ActionResult Create()
        {
            var model = new CreateBudgetViewModel
            {
                Id = BudgetId.Create().ToString(),
                Name = "New Budget",
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Create(CreateBudgetViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<CreateBudget>();
                handler.Handle(new CreateBudget
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetName = model.Name,
                    BudgetId = BudgetId.Create().ToString(),
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

    }

}
