using MyBudget.Domain.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MyBudget.Projections;

namespace MyBudget.Web.AspNet.Controllers
{
    public partial class MyBudgetController : AsyncController
    {
        UserId _userId;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            //var userId = Thread.CurrentPrincipal.Identity.Name;

            var userId = User.Identity.GetUserId();

            var users = ProjectionManager.GetUsersList();
            //var task = users.FindByIdAsync(userId);
            //var user = task.Result;
            var user = users.FindById(userId);
            if(user == null)
                throw new Exception("Utente non è riconosciuto dal sistema");

            _userId = new UserId(userId);
        }

        public UserId GetCurrentUserId()
        {
            return _userId;
        }

        protected ProjectionManager ProjectionManager 
        {
            get { return MvcApplication.ProjectionManager; }
        }

        protected CommandManager CommandManager
        {
            get { return MvcApplication.CommandManager; }
        }
	}

}