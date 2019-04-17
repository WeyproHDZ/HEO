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
using HeO.Libs;
using System.IO;
using System.Configuration;

namespace HeO.Controllers
{
    public class DepositMsController : BaseController
    {
        private VipdetailService vipdetailService;
        private ViprecordService viprecordService;
        public DepositMsController()
        {
            vipdetailService = new VipdetailService();
            viprecordService = new ViprecordService();
            System.Web.HttpContext.Current.Session.Add("href", "Deposit");
            System.Web.HttpContext.Current.Session["href"] = "Deposit";
        }
        // GET: DepositMs
        [CheckSession]
        public ActionResult Deposit()
        {
            Session["href"] = null;
            ViewBag.Vipdetail = vipdetailService.Get().OrderBy(o => o.Day);
            return View();
        }

        [CheckSession]
        [HttpPost]
        public ActionResult Deposit(Viprecord viprecord)
        {            
            Vipdetail data = vipdetailService.Get().Where(a => a.Money == viprecord.Money).FirstOrDefault();
            Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
            if(TryUpdateModel(viprecord , new string[] { "Money" , "Payway" }) && ModelState.IsValid)
            {
                viprecord.Viprecordid = Guid.NewGuid();
                viprecord.Memberid = Memberid;
                viprecord.Depositnumber = "heodeposit" + DateTime.Now.ToString("yyyyMMddss");
                viprecord.Day = data.Day;
                viprecord.Enddate = DateTime.Now;
                viprecord.Createdate = DateTime.Now;
                viprecord.Updatedate = DateTime.Now;
                viprecordService.Create(viprecord);
                viprecordService.SaveChanges();          
            }
            string CustomerURL = Url.Action("DepositSuccess", "Deposit");
            Ezpay.set_paramer(viprecord , CustomerURL);                    
            return RedirectToAction("DepositForm");
        }

        [CheckSession]        
        public ActionResult DepositForm()
        {
            string form = Ezpay.excute();
            ViewBag.Form = form;
            return View();
        }

        [CheckSession]
        public ActionResult DepositSuccess(string Number)
        {
            Ezpay.set_paramer(null, null, Number);
            return View();
        }
    }
}