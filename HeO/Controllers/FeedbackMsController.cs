using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Text;
using PagedList;
using HeO.Filters;
using HeO.Models;
using HeO.Service;
using System.IO;
using System.Configuration;
using System.Net;

namespace HeO.Controllers
{
    public class FeedbackMsController : BaseController
    {
        private FeedbackproductService feedbackproductService;
        private FeedbackrecordService feedbackrecordService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private MemberblacklistService memberblacklistService;
        private OrderfacebooklistService orderfacebooklistService;

        public FeedbackMsController()
        {
            feedbackproductService = new FeedbackproductService();
            feedbackrecordService = new FeedbackrecordService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberblacklistService = new MemberblacklistService();
            orderfacebooklistService = new OrderfacebooklistService();
        }

        [CheckSession]
        public ActionResult Feedbackcount()
        {            
            int i = 0;
            Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
            Members member = membersService.GetByID(Session["Memberid"]);
            IEnumerable<Feedbackproduct> FeedbackproductList = feedbackproductService.Get().OrderBy(o => o.Createdate);
            int[] Product = new int[FeedbackproductList.Count()];
            foreach (Feedbackproduct Feedbackproduct in FeedbackproductList)
            {
                if(orderfacebooklistService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Feedbackproductid == Feedbackproduct.Feedbackproductid).Count() == 0)
                {
                    i++;
                }
                else
                {
                    Product[i] = orderfacebooklistService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Feedbackproductid == Feedbackproduct.Feedbackproductid).Count();
                }
            }
            if (member.Isreal == true)
            {
                ViewBag.Levelid = memberlevelService.Get().Where(a => a.Levelname == "真人").FirstOrDefault().Levelid;
            }
            else
            {
                ViewBag.Levelid = memberlevelService.Get().Where(a => a.Levelname == "一般").FirstOrDefault().Levelid;
            }
            ViewBag.HeODate = member.Createdate.ToString("yyyy/MM/dd");                 // 加入HeO的時間
            ViewBag.Feedbackproduct = feedbackproductService.Get().OrderBy(o => o.Createdate);
            ViewBag.FeedbackCount = Product;
            return View();
        }

        [CheckSession]
        public ActionResult Feedbackrecord()
        {
            Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
            ViewBag.Feedbackrecord = feedbackrecordService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(o => o.Createdate);

            ViewBag.Membermoney = membersService.GetByID(Session["Memberid"]).Feedbackmoney;
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Feedbackrecord(Feedbackrecord feedbackrecord)
        {
            Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
            Memberblacklist blacklist = new Memberblacklist();           
            string ipaddress;
            ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if (ipaddress == "" || ipaddress == null)
            {
                ipaddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            Members Member = membersService.GetByID(Memberid);
            RegexStringValidator myRegexValidator = new RegexStringValidator(@"/^[0 - 9] *$/");
            if (feedbackrecord.Money > Member.Feedbackmoney || feedbackrecord.Money <= 0 || myRegexValidator.CanValidate(feedbackrecord.Money.GetType()))
            {
                blacklist.Account = Member.Account;
                blacklist.Memberid = Guid.Parse(Session["Memberid"].ToString());
                blacklist.Useragent = Request.UserAgent;
                blacklist.IP_Addr = ipaddress;
                memberblacklistService.Create(blacklist);
                memberblacklistService.SaveChanges();
                Session.RemoveAll();
                return RedirectToAction("Home", "HomeMs");
            }          
            IEnumerable<Feedbackrecord> old_data = feedbackrecordService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(o => o.Createdate);
            int count = old_data.Count();
            if(count == 0)
            {
                Session["Remains"] = Member.Feedbackmoney;
            }
            else
            {
                Session["Remains"] = old_data.FirstOrDefault().Remains;
            }
            Session["Money"] = feedbackrecord.Money;
            return RedirectToAction("Feedbacktransfer");
        }

        [CheckSession]
        public ActionResult Feedbacktransfer()
        {
            return View();
        }

        [CheckSession]
        public ActionResult Feedbackconfirm()
        {
            ViewBag.Money = Session["Money"];
            return View();
        }


        [CheckSession]
        public ActionResult Feedbacksuccess(Feedbackrecord feedbackrecord)
        {
            Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
            Members Member = membersService.GetByID(Session["Memberid"]);
            feedbackrecord.Feedbackid = Guid.NewGuid();
            feedbackrecord.Memberid = Memberid;
            feedbackrecord.Hdzaccount = (Session["hdz_account"]).ToString();
            feedbackrecord.Money = Convert.ToInt32(Session["Money"]);
            feedbackrecord.Remains = Member.Feedbackmoney - Convert.ToInt32(Session["Money"]);
            feedbackrecord.Createdate = DateTime.Now;
            feedbackrecord.Updatedate = DateTime.Now;
            feedbackrecordService.Create(feedbackrecord);
            feedbackrecordService.SaveChanges();

            /*** 更新會員回饋金 ****/
            Member.Feedbackmoney -= Convert.ToInt32(Session["Money"]);
            membersService.SpecificUpdate(Member, new string[] { "Feedbackmoney" });
            membersService.SaveChanges();
            return View();
        }
    }
}