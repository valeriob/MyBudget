using EventStore.ClientAPI;
using MyBudget.Projections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace MyBudget.Web.AspNet
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static ProjectionManager ProjectionManager;
        public static CommandManager CommandManager;
        

        protected void Application_Start()
        {
            var cs = ConnectionSettings.Create();
            
            var con = EventStoreConnection.Create(new IPEndPoint(IPAddress.Loopback, 1113));
   
            con.Connect();
            ProjectionManager = new ProjectionManager(con);
            ProjectionManager.Run();

            CommandManager = new CommandManager(con);

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
