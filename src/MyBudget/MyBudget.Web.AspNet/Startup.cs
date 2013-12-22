using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(MyBudget.Web.AspNet.Startup))]
namespace MyBudget.Web.AspNet
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
