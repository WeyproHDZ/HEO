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
    public class MemberMsController : BaseController
    {
        private MembersService membersService;
        private MemberauthorizationService memberauthorizationService;
        private MemberlevelService memberlevelService;
        private FeedbackproductService feedbackproductService;
        private ViprecordService viprecordService;

        public MemberMsController()
        {
            membersService = new MembersService();
            memberauthorizationService = new MemberauthorizationService();
            memberlevelService = new MemberlevelService();
            feedbackproductService = new FeedbackproductService();
            viprecordService = new ViprecordService();
        }
        // GET: MemberMs
        [CheckSession]
        public ActionResult Member()
        {
            IEnumerable<Feedbackproduct> Feedbackproduct = feedbackproductService.Get();            
            Members member = membersService.GetByID(Session["Memberid"]);
            IEnumerable<Memberauthorization> Memberauthorization = memberauthorizationService.Get().Where(a => a.Memberid == member.Memberid);
            if (member.Isreal == true)
            {
                ViewBag.Levelid = memberlevelService.Get().Where(a => a.Levelname == "真人").FirstOrDefault().Levelid;
            }
            else
            {
                ViewBag.Levelid = memberlevelService.Get().Where(a => a.Levelname == "一般").FirstOrDefault().Levelid;
            }
            ViewBag.Memberauthorization = Memberauthorization;
            ViewBag.Feedbackproduct = Feedbackproduct;
            ViewBag.Facebookstatus = member.Facebookstatus;
            return View();
        }
        [HttpPost]
        [CheckSession]
        public ActionResult Member(int Facebookstatus)
        {
            Members Member = membersService.GetByID(Session["Memberid"]);
            Member.Facebookstatus = Facebookstatus;
            membersService.SpecificUpdate(Member, new string[] { "Facebookstatus" });
            membersService.SaveChanges();
            return RedirectToAction("Member");
        }

        [CheckSession]
        public ActionResult Membervip()
        {
            Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
            ViewBag.Viprecord = viprecordService.Get().Where(a => a.Memberid == Memberid).Where(x => x.Status == 2);
            return View();
        }
    }
}