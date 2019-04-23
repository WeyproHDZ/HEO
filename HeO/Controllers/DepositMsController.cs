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
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace HeO.Controllers
{
    public class DepositMsController : BaseController
    {
        private VipdetailService vipdetailService;
        private ViprecordService viprecordService;
        private MembersService membersService;
        public DepositMsController()
        {
            vipdetailService = new VipdetailService();
            viprecordService = new ViprecordService();
            membersService = new MembersService();

            System.Web.HttpContext.Current.Session.Add("href", "Deposit");
            System.Web.HttpContext.Current.Session["href"] = "Deposit";
        }
        // GET: DepositMs
        [CheckSession]
        public ActionResult Deposit()
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
                viprecord.Depositnumber = "heodeposit" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                viprecord.Day = data.Day;
                viprecord.Enddate = DateTime.Now;                       // VIP截止日期，因客戶還沒進行繳費，所以將日期寫現在
                viprecord.Buydate = DateTime.Now;                       // 購買日期
                viprecord.Createdate = DateTime.Now;                    // 建立時間
                viprecord.Updatedate = DateTime.Now;                    // 更新時間
                viprecordService.Create(viprecord);
                viprecordService.SaveChanges();          
            }
            
            string CustomerURL = "http://3872af0a.ngrok.io/DepositMs/DepositSuccess";
            string NotifyURL = "http://3872af0a.ngrok.io/DepositMs/DepositReceive";

            int ts2 =  (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;      // 總秒數
            Ezpay.set_paramer(viprecord, CustomerURL, NotifyURL, ts2);
            return RedirectToAction("DepositForm");
        }

        [CheckSession]        
        public ActionResult DepositForm()
        {
            string form = Ezpay.excute();
            ViewBag.Form = form;
            return View();
        }

        [HttpPost]
        [CheckSession]
        public string DepositReceive(string Number)
        {
            return Number;
        }

        [HttpPost]
        [CheckSession]
        public ActionResult DepositSuccess(string TradeInfo)
        {
            string Reecive = Ezpay.DecryptAES256(TradeInfo);            
            var EzpayRecive = JsonConvert.DeserializeObject<dynamic>(Reecive); // 將Expay回傳的json格式轉成物件 
            if(EzpayRecive.Result.PaymentType == "CVS")
            {
                string DepositNumber = EzpayRecive.Result.MerchantOrderNo;         // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                 // 付款方式
                ViewBag.Amt =  EzpayRecive.Result.Amt;                             // 付款金額
                ViewBag.Day = viprecord.Day;                                       // 購買天數
                ViewBag.CodeNo = EzpayRecive.Result.CodeNo;                       // 超商編號
                ViewBag.DueDate = DateTime.Now.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");  // 付款期限


                /*** 將EZPAY編號、付款方式、超商編號寫入資料庫 ***/
                viprecord.Tradenumber = EzpayRecive.Result.TradeNo;
                viprecord.Paymenttype = EzpayRecive.Result.PaymentType;
                viprecord.Paymentnumber = EzpayRecive.Result.CodeNo;
                viprecord.Duedate = DateTime.Now.AddDays(7);
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Paymentnumber", "Duedate" });
                viprecordService.SaveChanges();
            }

        
            return View();
        }
    }
}