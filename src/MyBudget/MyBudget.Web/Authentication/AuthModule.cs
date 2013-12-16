using Nancy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MyBudget.Web.Authentication
{
    public class AuthModule : NancyModule
    {
        public AuthModule()
        {
            Get["/"] = x =>
            {
                return string.Concat("Hello ", x.name);
            };
            Get["/Login"] = x =>
            {
                return string.Concat("Do login ", x.name);
            };

            Get["/Private"] = x =>
            {
                return string.Concat("Private ", x.name);
            };
            //Get["/greet/{name}"] = x =>
            //{
            //    return string.Concat("Hello ", x.name);
            //};
        }
    }
}