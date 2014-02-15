using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class DynamicController : Controller
    {
        //
        // GET: /Dynamic/
        public virtual ActionResult Index()
        {
            return View();
        }

        //
        // GET: /Dynamic/Details/5
        public virtual ActionResult Details(int id)
        {
            return View();
        }

        //
        // GET: /Dynamic/Create
        public virtual ActionResult Create()
        {
            return View();
        }

        //
        // POST: /Dynamic/Create
        [HttpPost]
        public virtual ActionResult Create(FormCollection collection)
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
        // GET: /Dynamic/Edit/5
        public virtual ActionResult Edit(int id)
        {
            //_instance.Body.Ciao = 2;
            return View(_instance);
        }
        DynamicViewModel _instance = new DynamicViewModel 
        { 
            Name = "Ciao", 
            Body = new System.Dynamic.ExpandoObject(),
            Items = new[] { new TextBoxHelper("nome","ciao") }
        };
      
        //
        // POST: /Dynamic/Edit/5
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
        // GET: /Dynamic/Delete/5
        public virtual ActionResult Delete(int id)
        {
            return View();
        }

        //
        // POST: /Dynamic/Delete/5
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

    public class DynamicViewModel
    {
        public string Name { get; set; }
        public dynamic Body { get; set; }
        public IRenderHtml[] Items { get; set; }
    }

    public interface IRenderHtml
    {
        HtmlString Render();
    }

    public class TextBoxHelper : IRenderHtml
    {
        string _text;
        string _name;
        public TextBoxHelper(string name, string text)
        {
            _name = name;
            _text = text;
        }

        public HtmlString Render()
        {
            return new HtmlString(string.Format("<input type='text' value='{0}' id='{1}' name='{1}' >", _text, _name));
        }
    }

}
