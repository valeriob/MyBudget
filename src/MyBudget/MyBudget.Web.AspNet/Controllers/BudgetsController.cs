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
    public class BudgetsController : MyBudgetController
    {
        public ActionResult Index()
        {
            var readModel = MvcApplication.ProjectionManager.GetBudgetsList();
            var budgets = readModel.GetBudgetsUserCanView(GetCurrentUserId());

            var model = new BudgetListViewModel(budgets);

            return View(model);
        }

        public ActionResult Details(string id)
        {
            var readModel = MvcApplication.ProjectionManager.GetBudgetsList();
            var budget = readModel.GetBudgetById(new BudgetId(id));

            return View(budget);
        }


        public ActionResult Create()
        {
            var model = new CreateBudgetViewModel
            {
                Id = BudgetId.Create().ToString(),
                Name = "New Budget",
            };
            return View(model);
        }

        [HttpPost]
        //public ActionResult Create(FormCollection collection)
        public ActionResult Create(CreateBudgetViewModel model)
        {
            try
            {
                var handler = MvcApplication.CommandManager.Create<CreateBudget>();
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

        public ActionResult Edit(int id)
        {
            //var model = new BudgetViewModel
            //{

            //};
            return View();
        }

        [HttpPost]
        public ActionResult Edit(int id, FormCollection collection)
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


        public ActionResult Delete(int id)
        {
            return View();
        }


        [HttpPost]
        public ActionResult Delete(int id, FormCollection collection)
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
