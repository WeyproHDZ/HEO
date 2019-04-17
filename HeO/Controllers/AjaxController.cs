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
    public class AjaxController : BaseController
    {
        private MembersService membersSerice;
        private MemberauthorizationService memberauthorizationService;
        private VipdetailService vipdetailService;
        public AjaxController()
        {
            membersSerice = new MembersService();
            memberauthorizationService = new MemberauthorizationService();
            vipdetailService = new VipdetailService();
        }
        // GET: Ajax
        [CheckSession]
        [HttpPost]
        public JsonResult AjaxFeedbackconfirm(string hdz_account)
        {
            Session["hdz_account"] = hdz_account;
            return this.Json("success");
        }

        [CheckSession]
        [HttpPost]
        public JsonResult AjaxMember(Guid uuid)
        {
            Members Member = membersSerice.GetByID(Session["Memberid"]);
            Memberauthorization memberauthorization = memberauthorizationService.Get().Where( a => a.Memberid == Member.Memberid).Where( x => x.Feedbackproductid == uuid).FirstOrDefault();
            if(memberauthorization != null)
            {
                memberauthorization.Checked = !memberauthorization.Checked;
                memberauthorizationService.SaveChanges();
            }
            else
            {
                memberauthorization = new Memberauthorization();
                memberauthorization.Id = Guid.NewGuid();
                memberauthorization.Memberid = Member.Memberid;
                memberauthorization.Feedbackproductid = uuid;
                memberauthorization.Checked = true;
                memberauthorizationService.Create(memberauthorization);
                memberauthorizationService.SaveChanges();
            }
            return this.Json("success");
        }

        [CheckSession]
        [HttpPost]
        public JsonResult AjaxDeposit(int Money)
        {
            Vipdetail vipdetail = vipdetailService.Get().Where(o => o.Money == Money).FirstOrDefault();
            return this.Json(vipdetail);
        }
    }
}