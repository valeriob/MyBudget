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

        public virtual ActionResult Page(string id, string From, string To, int? pageIndex, string category)
        {
            DateTime? from = null;
            DateTime? to = null;

            if (string.IsNullOrEmpty(From) == false)
                from = DateTime.Parse(From);

            if (string.IsNullOrEmpty(To) == false)
                to = DateTime.Parse(To);

            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(id));
            var readModel = ProjectionManager.GetBudgetLinesProjection(id);
            var lines = readModel.GetAllLinesPaged(pageIndex.GetValueOrDefault(), from, to, category);

            var model = new BudgetLinesPagedViewModel(id, lines, from, to, categories, category);

            return View(model);
        }

        public virtual ActionResult Details(int id)
        {
            return View();
        }

        public virtual ActionResult Create(string id)
        {
            var model = Prepare_EditBudgetLineViewModel(id);
            
            return View(Views.Edit, model);
        }

        [HttpPost]
        public virtual ActionResult Create(EditBudgetLineViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var handler = CommandManager.Create<CreateLine>();
                    handler(model.PrepareCreateLine(GetCurrentUserId().ToString()));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("CurrencyISOCode", ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(Actions.Index(model.BudgetId));
            }
            else
            {
                var newmodel = Prepare_EditBudgetLineViewModel(model.BudgetId);
  
                newmodel.LoadUserInputFrom(model);

                return View(Views.Edit, newmodel);
            }
        }

        EditBudgetLineViewModel Prepare_EditBudgetLineViewModel(string id)
        {
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(id));
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(new Domain.Budgets.BudgetId(id));

            var model = new EditBudgetLineViewModel(budget.Name, id, budget.CurrencyISOCode, categories, budget.GetDistributionKeys(),
                Currencies.GetAll());
            return model;
        }

        public virtual ActionResult Edit(string budgetId, string lineId)
        {
            var categories = ProjectionManager.GetCategories().GetBudgetsCategories(new Domain.Budgets.BudgetId(budgetId));
            var budget = ProjectionManager.GetBudgetsList().GetBudgetById(new Domain.Budgets.BudgetId(budgetId));

            var model = new EditBudgetLineViewModel(budget.Name, budget.Id, budget.CurrencyISOCode, ProjectionManager.GetStreamEvents(lineId), categories, budget.GetDistributionKeys(), Currencies.GetAll());

            return View(model);
        }

        [HttpPost]
        public virtual ActionResult Edit(EditBudgetLineViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var handler = CommandManager.Create<UpdateLine>();
                    handler(model.PrepareUpdateLine(GetCurrentUserId().ToString()));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", ex.Message);
                }
            }

            if (ModelState.IsValid)
            {
                return RedirectToAction(Actions.Index(model.BudgetId));
            }
            else
            {
                var newmodel = Prepare_EditBudgetLineViewModel(model.BudgetId);

                newmodel.LoadUserInputFrom(model);

                return View(newmodel);
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
