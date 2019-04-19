using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using HeO.Models;
using HeO.Service;

namespace HeO.Controllers
{
    public class BaseController : Controller
    {
        protected string container = ConfigurationManager.AppSettings["azure.blob.container"];
        protected string url = ConfigurationManager.AppSettings["azure.blob.url"];
        protected string websitetitle = ConfigurationManager.AppSettings["websitetitle"];

        private ViprecordService viprecordService;

        public BaseController()
        {
            viprecordService = new ViprecordService();
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.BlobUrl = url + "/" + container + "/";

            base.OnActionExecuting(filterContext);
        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            
            if(Session["Memberid"] != null)           
            {
                Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
                Viprecord old_data = viprecordService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Status == 2).OrderByDescending(o => o.Createdate).FirstOrDefault();
                /*** 判斷該會員剩餘VIP天數 ***/
                if(old_data != null)
                {
                    Double Date = (old_data.Enddate - DateTime.Now).TotalDays;
                    if(Date < 0)
                    {
                        ViewBag.Date = 0;
                    }
                    else
                    {
                        ViewBag.Date = Convert.ToInt16(Math.Ceiling(Date));
                    }
                }

                /*** 判斷該會員是否有買過VIP ***/
                ViewBag.Checkvip = viprecordService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Status == 2).Count();
            }
            ViewBag.BlobUrl = url + "/" + container + "/";
            ViewBag.WebsiteTitle = websitetitle;
            ViewBag.AddActive = (ViewBag.CartItems > 0) ? "add-active" : "";   

            base.OnActionExecuted(filterContext);
        }
    }
}