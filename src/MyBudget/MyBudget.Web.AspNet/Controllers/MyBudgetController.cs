using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;

namespace MyBudget.Web.AspNet.Controllers
{
    public class MyBudgetController : Controller
    {
        UserId _userId;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var userId = Thread.CurrentPrincipal.Identity.Name;

            var users = MvcApplication.ProjectionManager.GetUsersList();
            var user = users.FindById(userId);
            if(user == null)
                throw new Exception("Utente non è riconosciuto dal sistema");

            _userId = new UserId(userId);
        }

        public UserId GetCurrentUserId()
        {
            return _userId;
        }

	}

}