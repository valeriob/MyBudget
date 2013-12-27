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
            var readModel = MvcApplication.ProjectionManager.GetBudgetLinesProjection(id);
            var lines = readModel.GetAllLines();
            var model = new BudgetLinesViewModel(id, lines);

            return View(model);
        }

        public virtual ActionResult Details(int id)
        {
            return View();
        }

        public virtual ActionResult Create(string id)
        {
            var model = new CreateBudgetLineViewModel 
            {
                BudgetId = id,
                LineId = LineId.Create(new MyBudget.Domain.Budgets.BudgetId(id)).ToString(),
                Date = DateTime.Now,
                CurrencyISOCode = Currencies.Euro().IsoCode,
            };
            return View(model);
        }

        [HttpPost]
        //public ActionResult Create(FormCollection collection)
        public virtual ActionResult Create(CreateBudgetLineViewModel model)
        {
            try
            {
                var handler = MvcApplication.CommandManager.Create<CreateLine>();
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
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Lines/Edit/5
        public virtual ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Lines/Edit/5
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

        //
        // GET: /Lines/Delete/5
        public virtual ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Lines/Delete/5
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
