using MyBudget.Commands;
using MyBudget.Domain.Lines;
using MyBudget.Domain.ValueObjects;
using MyBudget.Web.AspNet.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class LinesController : MyBudgetController
    {

        public virtual ActionResult Index(string id)
        {
            var readModel = ProjectionManager.GetBudgetLinesProjection(id);
            var lines = readModel.GetAllLines();
            var model = new BudgetLinesViewModel(id, lines);

            return View(model);
        }

        public virtual ActionResult Page(string id, string From, string To, int? pageIndex)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (string.IsNullOrEmpty(From) == false)
                from = DateTime.Parse(From);
            //from = From;
            if (string.IsNullOrEmpty(To) == false)
                to = DateTime.Parse(To);

            var readModel = ProjectionManager.GetBudgetLinesProjection(id);
            var lines = readModel.GetAllLinesPaged(pageIndex.GetValueOrDefault(), from, to);
            var model = new BudgetLinesPagedViewModel(id, lines, from, to);

            return View(model);
        }

        public virtual ActionResult Details(int id)
        {
            return View();
        }

        public virtual ActionResult Create(string id)
        {
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(id));
            var budgetName = ProjectionManager.GetBudgetsList().GetBudgetById(new Domain.Budgets.BudgetId(id)).Name;
            var model = new EditBudgetLineViewModel(budgetName, id, categories, Currencies.GetAll());
            
            return View(Views.Edit, model);
        }

        [HttpPost]
        public virtual ActionResult Create(EditBudgetLineViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<CreateLine>();
                handler.Handle(new CreateLine
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetId = model.BudgetId.ToString(),
                    LineId = model.LineId.ToString(),
                    Date = model.Date,
                    Amount = new Amount(Currencies.Parse(model.CurrencyISOCode), model.Amount),
                    Category = model.Category,
                    Description= model.Description,

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

        public virtual ActionResult Edit(string budgetId, string lineId)
        {
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(budgetId));
            var budgetName = ProjectionManager.GetBudgetsList().GetBudgetById(new Domain.Budgets.BudgetId(budgetId)).Name;
            var model = new EditBudgetLineViewModel(budgetName, ProjectionManager.GetStreamEvents(lineId), categories, Currencies.GetAll());

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Edit(EditBudgetLineViewModel model)
        {
            try
            {
                var handler = CommandManager.Create<UpdateLine>();
                handler.Handle(new UpdateLine
                {
                    UserId = GetCurrentUserId().ToString(),
                    BudgetId = model.BudgetId.ToString(),
                    LineId = model.LineId.ToString(),

                    Date = model.Date,
                    Amount = new Amount(Currencies.Parse(model.CurrencyISOCode), model.Amount),
                    Category = model.Category,
                    Description = model.Description,

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
