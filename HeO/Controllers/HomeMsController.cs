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

namespace HeO.Controllers
{
    public class HomeMsController : BaseController
    {
        private NewsService newsService;
        private NewslangService newslangService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private MemberlevelcooldownService memberlevelcooldownService;
        public HomeMsController()
        {
            newsService = new NewsService();
            newslangService = new NewslangService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberlevelcooldownService = new MemberlevelcooldownService();
        }
        // GET: HomeMs
        /*** 首頁 ***/
        public ActionResult Home()
        {
            ViewBag.News = newsService.Get().Take(2).OrderBy(o => o.Createdate);
            return View();
        }

        /*** 關於我們 ***/
        public ActionResult About()
        {
            return View();
        }

        /*** 登入畫面 ****/
        public ActionResult Login()
        {
            if (Convert.ToBoolean(Session["IsLogin"]))
            {
                return RedirectToAction("Home");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Login(Members members)
        {
            IEnumerable<Members> old_members = membersService.Get();
            IEnumerable<Memberlevelcooldown> memberlevelcooldown = memberlevelcooldownService.Get().OrderByDescending(o => o.Cooldowntime);  // 會員層級冷卻時間由長到短
            
            var Level = memberlevelcooldown.FirstOrDefault().Levelid;  // 撈第一筆資料的id
            foreach(Members old_member in old_members)
            {
                if (members.Account == old_member.Account)
                {
                    if (Session["href"] == null)
                    {
                        if (old_member.Facebookstatus == 0)
                        {
                            Session["IsLogin"] = true;
                            Session["Memberid"] = old_member.Memberid;
                            Session["Lastdate"] = DateTime.Now.ToShortDateString();
                            return RedirectToAction("Certified");
                        }
                        else
                        {
                            Session["IsLogin"] = true;
                            Session["Memberid"] = old_member.Memberid;
                            Session["Lastdate"] = DateTime.Now.ToShortDateString();
                            return RedirectToAction("Order", "OrderMs");
                        }
                    }
                    else
                    {
                        Session["IsLogin"] = true;
                        Session["Memberid"] = old_member.Memberid;
                        Session["Lastdate"] = DateTime.Now.ToShortDateString();
                        return RedirectToAction("Deposit", "DepositMs");
                    }
                }
            }
            if(TryUpdateModel(members , new string[] { "Account" , "Password" }))
            {
                members.Memberid = Guid.NewGuid();
                members.Levelid = Level;
                members.Createdate = DateTime.Now;
                members.Updatedate = DateTime.Now;
                members.Lastdate = DateTime.Now.ToShortDateString();
                members.Facebooklink = members.Account;
                membersService.Create(members);
                membersService.SaveChanges();
            }
            Session["IsLogin"] = true;
            Session["Memberid"] = members.Memberid;
            return RedirectToAction("Certified");
        }

        [CheckSession]
        public ActionResult Logout()
        {
            Session["IsLogin"] = false;
            return RedirectToAction("Home", "HomeMs");
        }

        [CheckSession]
        public ActionResult Certified()
        {
            /*** 更新會員最近登入時間 ***/
            if(Session["Lastdate"] != null)
            {
                Members Member = membersService.GetByID(Session["Memberid"]);
                Member.Lastdate = Session["Lastdate"].ToString();
                membersService.SpecificUpdate(Member, new string[] { "Lastdate" });
                membersService.SaveChanges();
                Session["Lastdate"] = null;
            }
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Certified(Members member)
        {
            membersService.SpecificUpdate(member, new string[] { "Facebookstatus" });
            membersService.SaveChanges();

            return RedirectToAction("Order", "OrderMs");
        }
    }
}