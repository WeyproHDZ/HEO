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
    public class DepositMsController : BaseController
    {
        private VipdetailService vipdetailService;

        public DepositMsController()
        {
            vipdetailService = new VipdetailService();
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
    }
}