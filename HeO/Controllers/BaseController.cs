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
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        public BaseController()
        {
            viprecordService = new ViprecordService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
        }
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            ViewBag.BlobUrl = url + "/" + container + "/";

            base.OnActionExecuting(filterContext);
        }
        protected override void OnActionExecuted(ActionExecutedContext filterContext)
        {
            if (Session["Memberid"] != null)           
            {
                Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
                Guid Normalid = memberlevelService.Get().Where(a => a.Levelname == "一般").FirstOrDefault().Levelid;      // 一般會員的ID
                Members member = membersService.GetByID(Memberid);                                                        // 該會員的詳細資料
                Viprecord viprecord = viprecordService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Status == 2).OrderByDescending(o => o.Enddate).FirstOrDefault();
                /*** 判斷該會員剩餘VIP天數 ***/
                if(viprecord != null)
                {
                    

                    Double Day = (viprecord.Enddate - DateTime.Now).TotalDays;
                    if(Day > 0)
                    {
                        ViewBag.Remainday = Convert.ToInt16(Math.Ceiling(Day));
                    }
                    else
                    {
                        ViewBag.Remainday = 0;
                        if(member.Levelid != Normalid)
                        {
                            member.Levelid = Normalid;
                            membersService.SpecificUpdate(member, new string[] { "Levelid" });
                            membersService.SaveChanges();
                        }
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