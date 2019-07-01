using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HeO
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            RouteConfig.RegisterRoutes(RouteTable.Routes);
        }

        //void Application_Start(object sebder, EventArgs e)
        //{
        //    Application["OnlineUsers"] = 0;
        //}

        //void Session_Start(object sender, EventArgs e)
        //{
        //    Application.Lock();
        //    Application["OnlineUsers"] = (int)Application["OnlineUsers"] + 1;
        //    Application.UnLock();
        //}

        //void Session_End(object sender, EventArgs e)
        //{
        //    Application.Lock();
        //    Application["OnlineUsers"] = (int)Application["OnlineUsers"] - 1;
        //    Application.UnLock();
        //}
    }
}


