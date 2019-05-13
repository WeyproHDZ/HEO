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
using System.Web.Caching;

namespace HeO.Controllers
{
    public class DepositMsController : BaseController
    {
        private VipdetailService vipdetailService;
        private ViprecordService viprecordService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        public DepositMsController()
        {
            vipdetailService = new VipdetailService();
            viprecordService = new ViprecordService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();

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
                viprecord.Depositnumber = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                viprecord.Day = data.Day;
                viprecord.Enddate = DateTime.Now;                       // VIP截止日期，因客戶還沒進行繳費，所以將日期寫現在
                viprecord.Buydate = DateTime.Now;                       // 購買日期
                viprecord.Createdate = DateTime.Now;                    // 建立時間
                viprecord.Updatedate = DateTime.Now;                    // 更新時間
                viprecordService.Create(viprecord);
                viprecordService.SaveChanges();          
            }
            
            string CustomerURL = "http://heofrontend.4webdemo.com/DepositMs/DepositSuccess";
            string NotifyURL = "http://heofrontend.4webdemo.com/DepositMs/DepositReceive";

            int TimeStamp =  (int)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;      // 總秒數
            Ezpay.set_paramer(viprecord, CustomerURL, NotifyURL, TimeStamp);
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
            string Receive = Ezpay.DecryptAES256(TradeInfo);
            var EzpayRecive = JsonConvert.DeserializeObject<dynamic>(Receive); // 將Ezpay回傳的json格式轉成物件
            if(EzpayRecive.Result.PaymentType == "CVS")
            {
                string DepositNumber = EzpayRecive.Result.MerchantOrderNo;         // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                 // 付款方式
                ViewBag.Amt =  EzpayRecive.Result.Amt;                             // 付款金額
                ViewBag.Day = viprecord.Day;                                       // 購買天數
                ViewBag.CodeNo = EzpayRecive.Result.CodeNo;                        // 超商編號
                ViewBag.DueDate = DateTime.Now.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");  // 付款期限


                /*** 將EZPAY編號、付款方式、超商編號、繳費期限寫入資料庫 ***/
                viprecord.Tradenumber = EzpayRecive.Result.TradeNo;
                viprecord.Paymenttype = EzpayRecive.Result.PaymentType;
                viprecord.Paymentnumber = EzpayRecive.Result.CodeNo;
                viprecord.Duedate = DateTime.Now.AddDays(7);
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Paymentnumber", "Duedate" });
                viprecordService.SaveChanges();
            }
            else if(EzpayRecive.Result.PaymentType == "VACC")
            {
                string DepositNumber = EzpayRecive.Result.MerchantOrderNo;          // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                                          // 付款方式
                ViewBag.Amt = EzpayRecive.Result.Amt;                                                       // 付款金額
                ViewBag.Day = viprecord.Day;                                                                // 購買天數
                ViewBag.CodeNo = EzpayRecive.Result.CodeNo;                                                 // 金融繳費代碼
                ViewBag.EscrowBank = EzpayRecive.Result.EscrowBank;                                         // 銀行英文代碼
                ViewBag.BankCode = EzpayRecive.Result.BankCode;                                             // 銀行數字代碼
                ViewBag.DueDate = DateTime.Now.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");                  //繳費期限

                /*** 將EZPAY編號、付款方式、金融機構代碼、金融繳費代碼、繳費期限寫入資料庫 ****/
                viprecord.Tradenumber = EzpayRecive.Result.TradNo;
                viprecord.Paymenttype = EzpayRecive.Result.PaymentType;
                viprecord.Bankcode = EzpayRecive.Result.Bankcode;
                viprecord.Paymentnumber = EzpayRecive.Result.CodeNo;
                viprecord.Duedate = DateTime.Now.AddDays(7);
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Bankcode", "Paymentnumber", "Duedate" });
                viprecordService.SaveChanges();
            }
            else if(EzpayRecive.Result.PaymentType == "CREDIT")
            {
                string DepositNumber = EzpayRecive.Result.MerchantOrderNo;          // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                                          // 付款方式
                ViewBag.Status = EzpayRecive.Message;                                                       // 付款狀態
                ViewBag.Amt = EzpayRecive.Result.Amt;                                                       // 付款金額
                ViewBag.Day = viprecord.Day;                                                                // 購買天數
                ViewBag.OrderDate = EzpayRecive.Result.PayTime;                                             // 付款日期
                /*** 將付款方式、EZPAY交易編號、開始時間、截止時間 ***/
                Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
                Viprecord old_data = viprecordService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(x => x.Enddate).FirstOrDefault();
                viprecord.Tradenumber = EzpayRecive.Result.TradNo;
                viprecord.Paymenttype = EzpayRecive.Result.PaymentType;
                viprecord.Status = 2;
                if (old_data != null)
                {
                    if(old_data.Enddate < DateTime.Now)
                    {
                        viprecord.Startdate = DateTime.Now;
                        viprecord.Enddate = DateTime.Now.AddDays(Convert.ToDouble(viprecord.Day));
                    }
                    else
                    {
                        viprecord.Startdate = old_data.Enddate;
                        viprecord.Enddate = old_data.Enddate.AddDays(Convert.ToDouble(viprecord.Day));
                    }
                }
                else
                {
                    viprecord.Startdate = DateTime.Now;
                    viprecord.Enddate = DateTime.Now.AddDays(Convert.ToDouble(viprecord.Day));
                }
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Startdate", "Enddate", "Status" });
                viprecordService.SaveChanges();

                /*** 更新會員層級狀態 ***/
                Members member = membersService.GetByID(Memberid);
                Memberlevel memberlevel = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault();
                member.Levelid = memberlevel.Levelid;
                membersService.SpecificUpdate(member, new string[] { "Levelid" });
                membersService.SaveChanges();
            }
            return View();
        }
    }
}