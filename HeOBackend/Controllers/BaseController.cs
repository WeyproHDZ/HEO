using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeOBackend;
using HeO.Models;
using HeO.Service;

namespace HeOBackend.Controllers
{
    public class BaseController : Controller
    {
        private LimsSerivce  limsService;
        protected string container = ConfigurationManager.AppSettings["azure.blob.container"];
        protected string url = ConfigurationManager.AppSettings["azure.blob.url"];

        public BaseController()
        {
            limsService = new LimsSerivce();
        }

        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            ViewBag.SiteLinks = limsService.Get().OrderBy(p => p.ParentID).ThenBy(s => s.Sort);
            ViewBag.BlobUrl = url + "/" + container + "/";
            base.OnActionExecuted(filterContext);
        }
    }
}