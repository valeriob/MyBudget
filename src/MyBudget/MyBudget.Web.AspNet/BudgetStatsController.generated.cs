// <auto-generated />
// This file was generated by a T4 template.
// Don't change it directly as your change would get overwritten.  Instead, make changes
// to the .tt file (i.e. the T4 template) and save it to regenerate this file.

// Make sure the compiler doesn't complain about missing Xml comments
#pragma warning disable 1591
#region T4MVC

using System;
using System.Diagnostics;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Mvc.Ajax;
using System.Web.Mvc.Html;
using System.Web.Routing;
using T4MVC;
namespace MyBudget.Web.AspNet.Controllers
{
    public partial class BudgetStatsController
    {
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public BudgetStatsController() { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected BudgetStatsController(Dummy d) { }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToAction(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoute(callInfo.RouteValueDictionary);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        protected RedirectToRouteResult RedirectToActionPermanent(ActionResult result)
        {
            var callInfo = result.GetT4MVCResult();
            return RedirectToRoutePermanent(callInfo.RouteValueDictionary);
        }

        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult Index()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ByCategory()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ByCategory);
        }
        [NonAction]
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public virtual System.Web.Mvc.ActionResult ByCategoryInTime()
        {
            return new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ByCategoryInTime);
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public BudgetStatsController Actions { get { return MVC.BudgetStats; } }
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Area = "";
        [GeneratedCode("T4MVC", "2.0")]
        public readonly string Name = "BudgetStats";
        [GeneratedCode("T4MVC", "2.0")]
        public const string NameConst = "BudgetStats";

        static readonly ActionNamesClass s_actions = new ActionNamesClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionNamesClass ActionNames { get { return s_actions; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNamesClass
        {
            public readonly string Index = "Index";
            public readonly string ByCategory = "ByCategory";
            public readonly string ByCategoryInTime = "ByCategoryInTime";
        }

        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionNameConstants
        {
            public const string Index = "Index";
            public const string ByCategory = "ByCategory";
            public const string ByCategoryInTime = "ByCategoryInTime";
        }


        static readonly ActionParamsClass_Index s_params_Index = new ActionParamsClass_Index();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_Index IndexParams { get { return s_params_Index; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_Index
        {
            public readonly string id = "id";
        }
        static readonly ActionParamsClass_ByCategory s_params_ByCategory = new ActionParamsClass_ByCategory();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ByCategory ByCategoryParams { get { return s_params_ByCategory; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ByCategory
        {
            public readonly string budgetId = "budgetId";
            public readonly string From = "From";
            public readonly string To = "To";
        }
        static readonly ActionParamsClass_ByCategoryInTime s_params_ByCategoryInTime = new ActionParamsClass_ByCategoryInTime();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ActionParamsClass_ByCategoryInTime ByCategoryInTimeParams { get { return s_params_ByCategoryInTime; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ActionParamsClass_ByCategoryInTime
        {
            public readonly string budgetId = "budgetId";
            public readonly string From = "From";
            public readonly string To = "To";
            public readonly string groupBy = "groupBy";
        }
        static readonly ViewsClass s_views = new ViewsClass();
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public ViewsClass Views { get { return s_views; } }
        [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
        public class ViewsClass
        {
            static readonly _ViewNamesClass s_ViewNames = new _ViewNamesClass();
            public _ViewNamesClass ViewNames { get { return s_ViewNames; } }
            public class _ViewNamesClass
            {
                public readonly string ByCategory = "ByCategory";
                public readonly string Index = "Index";
            }
            public readonly string ByCategory = "~/Views/BudgetStats/ByCategory.cshtml";
            public readonly string Index = "~/Views/BudgetStats/Index.cshtml";
        }
    }

    [GeneratedCode("T4MVC", "2.0"), DebuggerNonUserCode]
    public partial class T4MVC_BudgetStatsController : MyBudget.Web.AspNet.Controllers.BudgetStatsController
    {
        public T4MVC_BudgetStatsController() : base(Dummy.Instance) { }

        partial void IndexOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string id);

        public override System.Web.Mvc.ActionResult Index(string id)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.Index);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "id", id);
            IndexOverride(callInfo, id);
            return callInfo;
        }

        partial void ByCategoryOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string budgetId, string From, string To);

        public override System.Web.Mvc.ActionResult ByCategory(string budgetId, string From, string To)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ByCategory);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "budgetId", budgetId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "From", From);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "To", To);
            ByCategoryOverride(callInfo, budgetId, From, To);
            return callInfo;
        }

        partial void ByCategoryInTimeOverride(T4MVC_System_Web_Mvc_ActionResult callInfo, string budgetId, string From, string To, string groupBy);

        public override System.Web.Mvc.ActionResult ByCategoryInTime(string budgetId, string From, string To, string groupBy)
        {
            var callInfo = new T4MVC_System_Web_Mvc_ActionResult(Area, Name, ActionNames.ByCategoryInTime);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "budgetId", budgetId);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "From", From);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "To", To);
            ModelUnbinderHelpers.AddRouteValues(callInfo.RouteValueDictionary, "groupBy", groupBy);
            ByCategoryInTimeOverride(callInfo, budgetId, From, To, groupBy);
            return callInfo;
        }

    }
}

#endregion T4MVC
#pragma warning restore 1591
