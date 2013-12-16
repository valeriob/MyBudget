using System;
using Microsoft.Owin;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Microsoft.Owin.Security;

//  http://stackoverflow.com/questions/18885569/how-do-i-ignore-the-identity-framework-magic-and-just-use-the-owin-auth-middlewa

namespace MyBudget.Web
{
    public partial class Startup
    {
        public void ConfigureAuth(IAppBuilder app)
        {
            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "Application",
                AuthenticationMode = AuthenticationMode.Passive,
                LoginPath = new PathString("/Login"),
                LogoutPath = new PathString("/Logout"),
            });

            app.SetDefaultSignInAsAuthenticationType("External");

            app.UseCookieAuthentication(new CookieAuthenticationOptions
            {
                AuthenticationType = "External",
                AuthenticationMode = AuthenticationMode.Passive,
                CookieName = CookieAuthenticationDefaults.CookiePrefix + "External",
                ExpireTimeSpan = TimeSpan.FromMinutes(5),
            });

            app.UseGoogleAuthentication();
        }

    }

}
