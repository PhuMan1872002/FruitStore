using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Security;

namespace DepartMentStore
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
        protected void Application_AuthenticateRequest(Object sender, EventArgs e)
        {
            //if (HttpContext.Current.User != null)
            //{
            //    // Kiểm tra xem người dùng đã được xác thực chưa
            //    if (HttpContext.Current.User.Identity.IsAuthenticated)
            //    {
                    HttpCookie authCookie = Context.Request.Cookies[FormsAuthentication.FormsCookieName];

                    if (authCookie != null)
                    {
                        FormsAuthenticationTicket authTicket = FormsAuthentication.Decrypt(authCookie.Value);

                        if (authTicket != null && !authTicket.Expired)
                        {
                            // Lấy role từ UserData trong ticket và ánh xạ vào context
                            string[] roles = new string[] { authTicket.UserData }; // Lấy role từ FormsAuthenticationTicket
                            Context.User = new System.Security.Principal.GenericPrincipal(new FormsIdentity(authTicket), roles);
                        }
                    }
            //    }
            //}
        }

    }
}
