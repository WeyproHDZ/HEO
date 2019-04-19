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
        private FeedbackproductService feedbackproductService;
        private ViprecordService viprecordService;

        public MemberMsController()
        {
            membersService = new MembersService();
            feedbackproductService = new FeedbackproductService();
            viprecordService = new ViprecordService();
        }
        // GET: MemberMs
        [CheckSession]
        public ActionResult Member()
        {
            IEnumerable<Feedbackproduct> Feedbackproduct = feedbackproductService.Get();
            Members member = membersService.GetByID(Session["Memberid"]);

            ViewBag.Feedbackproduct = Feedbackproduct;
            ViewBag.Facebookstatus = member.Facebookstatus;
            ViewBag.Account = member.Account;
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