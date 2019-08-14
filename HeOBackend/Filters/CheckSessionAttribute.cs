using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HeO.Models;
using HeO.Service;

namespace HeOBackend
{
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public bool IsAuth { get; set; }

        private LimsSerivce limsService;
        private AdminLimsService adminlimsService;

        public CheckSessionAttribute()
        {
            limsService = new LimsSerivce();
            adminlimsService = new AdminLimsService();
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {

            bool islogin = (context.HttpContext.Session.Contents["IsLogin"] == null) ? false : (bool)context.HttpContext.Session.Contents["IsLogin"];

            if (!islogin)
            {
                context.HttpContext.Session.Add("IsLogin", false);

                RouteValueDictionary redirectTargetDictionary = new RouteValueDictionary();
                redirectTargetDictionary.Add("action", "Login");
                redirectTargetDictionary.Add("controller", "Main");

                context.Result = new RedirectToRouteResult(redirectTargetDictionary);
                return;
            }

            if (IsAuth)
            {
                int adminid = (int)context.HttpContext.Session.Contents["AdminID"];

                if (adminid != 888)
                {
                    string action = (string)context.RequestContext.RouteData.Values["action"];
                    string controller = (string)context.RequestContext.RouteData.Values["controller"];

                    string ac = action;
                    ac = ac.Replace("AddNo", "Add");
                    ac = ac.Replace("EditNo", "Edit");
                    ac = ac.Replace("Add", "");
                    ac = ac.Replace("Edit", "");
                    ac = ac.Replace("Delete", "");
                    ac = ac.Replace("Result", "");
                    ac = ac.Replace("Export", "");
                    ac = ac.Replace("Sort", "");
                    ac = ac.Replace("Brandphotos", "Brands");
                    ac = ac.Replace("Productphotos", "Products");
                    ac = ac.Replace("Eventphotos", "Events");
                    ac = ac.Replace("details", "");
                    ac = ac.Replace("AllAuth", "");
                    ac = ac.Replace("Auth", "");
                    ac = ac.Replace("ErrorAuth", "");
                    ac = ac.Replace("Upload", "");
                    ac = ac.Replace("Viewfacebooklist", "");
                    Lims lim = limsService.Get().Where(a => a.Key.Contains(controller)).FirstOrDefault();
                    int limid = limsService.Get().Where(a => a.Key.Contains(ac) && a.ParentID == lim.LimID).Select(a => a.LimID).FirstOrDefault();

                    AdminLims adminlim = adminlimsService.Get().Where(a => a.AdminID == adminid && a.LimID == limid).FirstOrDefault();

                    if (adminlim == null)
                    {
                        context.Result = new RedirectResult("/Error/Validation");
                        return;
                    }

                    if (!adminlim.IsAdd && action.Contains("Add"))
                    {
                        context.Result = new RedirectResult("/Error/Validation");
                        return;
                    }

                    if (!adminlim.IsUpdate && action.Contains("Edit"))
                    {
                        context.Result = new RedirectResult("/Error/Validation");
                        return;
                    }

                    if (!adminlim.IsDelete && action.Contains("Delete"))
                    {
                        context.Result = new RedirectResult("/Error/Validation");
                        return;
                    }
                }
            }

            base.OnActionExecuting(context);
        }
    }
}