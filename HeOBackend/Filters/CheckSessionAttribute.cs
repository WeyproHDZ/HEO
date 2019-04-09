using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace HeOBackend
{
    public class CheckSessionAttribute : ActionFilterAttribute
    {
        public bool IsAuth { get; set; }

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
                    ac = ac.Replace("Add", "");
                    ac = ac.Replace("Edit", "");
                    ac = ac.Replace("Delete", "");
                    ac = ac.Replace("Result", "");
                    ac = ac.Replace("Export", "");

                    //Lims lim = _unitofWork.LimsRepository.Get(filter: a => a.Key == controller).FirstOrDefault();
                    //int limid = _unitofWork.LimsRepository.Get(filter: a => a.Key == ac && a.ParentID == lim.LimID).Select(a => a.LimID).FirstOrDefault();

                    //AdminLims adminlim = _unitofWork.AdminLimsRepository.Get(filter: a => a.AdminID == adminid && a.LimID == limid).FirstOrDefault();

                    //if (adminlim == null)
                    //{
                    //    context.Result = new RedirectResult("/Error/Validation");
                    //    return;
                    //}

                    //if (!adminlim.IsAdd && action.Contains("Add"))
                    //{
                    //    context.Result = new RedirectResult("/Error/Validation");
                    //    return;
                    //}

                    //if (!adminlim.IsUpdate && action.Contains("Edit"))
                    //{
                    //    context.Result = new RedirectResult("/Error/Validation");
                    //    return;
                    //}

                    //if (!adminlim.IsDelete && action.Contains("Delete"))
                    //{
                    //    context.Result = new RedirectResult("/Error/Validation");
                    //    return;
                    //}
                }
            }

            base.OnActionExecuting(context);
        }
    }
}