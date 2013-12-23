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

        //
        // GET: /Budgets/
        public ActionResult Index()
        {
            var readModel = MvcApplication.ProjectionManager.GetBudgetsList();
            var budgets = readModel.GetBudgetsUserCanView(GetCurrentUserId());

            var model = new BudgetListViewModel(budgets);

            return View(model);
        }

        //
        // GET: /Budgets/Details/5
        public ActionResult Details(string id)
        {
            var readModel = MvcApplication.ProjectionManager.GetBudgetsList();
            var budget = readModel.GetBudgetById(new BudgetId(id));

            return View(budget);
        }

        //
        // GET: /Budgets/Create
        public ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Budgets/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                // TODO: Add insert logic here

                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        //
        // GET: /Budgets/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        //
        // POST: /Budgets/Edit/5
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

        //
        // GET: /Budgets/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Budgets/Delete/5
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
