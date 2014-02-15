using MyBudget.Commands;
using MyBudget.Web.AspNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class CategoryController : MyBudgetController
    {
        public virtual ActionResult Index(string id)
        {
            var categories = base.ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(id));

            return View(categories);
        }


        public virtual ActionResult Details(string id)
        {
            // var readModel = ProjectionManager.GetCategories();
            // var budget = readModel.GetBudgetsCategories()
            //return View(budget);
            return View();
        }


        public virtual ActionResult Create(string budgetId)
        {
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(new MyBudget.Domain.Budgets.BudgetId(budgetId));
            var model = new CategoryViewModel
            {
                BudgetId = budgetId,
                BudgetName = budget.Name,
                Id = "Category-" + Guid.NewGuid().ToString(),
                Name = "New Category",
                Description = "Description",
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Create(CategoryViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<CreateCategory>();
                handler.Handle(new CreateCategory
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetId = model.BudgetId,
                    CategoryName = model.Name,
                    CategoryDescription = model.Description,
                    CategoryId = model.Id,

                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                });
                return RedirectToAction(Actions.Index(model.BudgetId));
            }
            catch
            {
                return View();
            }
        }


        public virtual ActionResult Edit(string budgetId, string categoryId)
        {
            var bid = new Domain.Budgets.BudgetId(budgetId);
            var category = ProjectionManager.GetCategories().GetBudgetsCategories(bid)
                .Single(r => r.Id == categoryId);
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(bid);
            var model = new CategoryViewModel
            {
                Id = categoryId,
                BudgetId = budgetId,
                BudgetName = budget.Name,
                Description = category.Description,
                Name = category.Name,
            };
            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Edit(CategoryViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<UpdateCategory>();
                handler.Handle(new UpdateCategory
                {
                    UserId = GetCurrentUserId().ToString(),
                    CategoryName = model.Name,
                    CategoryDescription = model.Description,
                    CategoryId = model.Id,

                    Id = Guid.NewGuid(),
                    Timestamp = DateTime.Now,
                });
                return RedirectToAction(Actions.Index(model.BudgetId));
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