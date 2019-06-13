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
        private ReturnstatusService returnstatusService;
        public DepositMsController()
        {
            vipdetailService = new VipdetailService();
            viprecordService = new ViprecordService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            returnstatusService = new ReturnstatusService();

            System.Web.HttpContext.Current.Session.Add("href", "Deposit");
            System.Web.HttpContext.Current.Session["href"] = "Deposit";
        }
        // GET: DepositMs
        [CheckSession]
        public ActionResult Deposit()
        {
            /*** 將該會員的Facebook連結寫進去 ***/
            if (Session["Facebooklink"] != null)
            {
                Members members = membersService.GetByID(Session["Memberid"]);
                members.Facebooklink = "https://www.facebook.com/profile.php?id=" + Session["Facebooklink"];
                membersService.SpecificUpdate(members, new string[] { "Facebooklink" });
                membersService.SaveChanges();
                Session.Remove("Facebooklink");
            }
            Session.Remove("href");            
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
            Newebpay.set_paramer(viprecord, CustomerURL, NotifyURL, TimeStamp);
            return RedirectToAction("DepositForm");
        }

        [CheckSession]        
        public ActionResult DepositForm()
        {
            string form = Newebpay.excute();
            ViewBag.Form = form;
            return View();
        }

        [HttpPost]
        public void DepositReceive(string TradeInfo)
        {
            string Receive = Newebpay.DecryptAES256(TradeInfo);
            var NewebpayRecive = JsonConvert.DeserializeObject<dynamic>(Receive); // 將Newebpay回傳的json格式轉成物件
            string DepositNumber = NewebpayRecive.Result.MerchantOrderNo;          // 取得儲值編號
            Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
            Returnstatus returnstatus = new Returnstatus();
            returnstatus.Text = Receive;
            returnstatus.Createdate = DateTime.Now;
            returnstatusService.Create(returnstatus);
            returnstatusService.SaveChanges();
            if (NewebpayRecive.Result.PaymentType == "VACC")
            {
                /*** 將付款方式、Newebpay交易編號、開始時間、截止時間 ***/
                Viprecord old_data = viprecordService.Get().Where(a => a.Memberid == viprecord.Memberid).OrderByDescending(x => x.Enddate).FirstOrDefault();
                viprecord.Paymenttype = NewebpayRecive.Result.PaymentType;
                viprecord.Status = 2;
                if (old_data != null)
                {
                    if (old_data.Enddate < DateTime.Now)
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
                viprecordService.SpecificUpdate(viprecord, new string[] {"Paymenttype", "Startdate", "Enddate", "Status" });
                viprecordService.SaveChanges();

                /*** 更新會員層級狀態 ***/
                Members member = membersService.GetByID(viprecord.Memberid);
                Memberlevel memberlevel = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault();
                member.Levelid = memberlevel.Levelid;
                membersService.SpecificUpdate(member, new string[] { "Levelid" });
                membersService.SaveChanges();
            }
            else if(NewebpayRecive.Result.PaymentType == "CVS")
            {
                /*** 將付款方式、Newebpay交易編號、開始時間、截止時間 ***/
                Viprecord old_data = viprecordService.Get().Where(a => a.Memberid == viprecord.Memberid).OrderByDescending(x => x.Enddate).FirstOrDefault();
                viprecord.Paymenttype = NewebpayRecive.Result.PaymentType;
                viprecord.Status = 2;
                if (old_data != null)
                {
                    if (old_data.Enddate < DateTime.Now)
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
                viprecordService.SpecificUpdate(viprecord, new string[] {"Paymenttype", "Startdate", "Enddate", "Status" });
                viprecordService.SaveChanges();

                /*** 更新會員層級狀態 ***/
                Members member = membersService.GetByID(viprecord.Memberid);
                Memberlevel memberlevel = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault();
                member.Levelid = memberlevel.Levelid;
                membersService.SpecificUpdate(member, new string[] { "Levelid" });
                membersService.SaveChanges();
            }
        }

        [HttpPost]
        [CheckSession]
        public ActionResult DepositSuccess(string TradeInfo)
        {
            string Receive = Newebpay.DecryptAES256(TradeInfo);
            var NewebpayRecive = JsonConvert.DeserializeObject<dynamic>(Receive); // 將Newebpay回傳的json格式轉成物件
            if(NewebpayRecive.Result.PaymentType == "CVS")
            {
                string DepositNumber = NewebpayRecive.Result.MerchantOrderNo;         // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                 // 付款方式
                ViewBag.Amt =  NewebpayRecive.Result.Amt;                             // 付款金額
                ViewBag.Day = viprecord.Day;                                       // 購買天數
                ViewBag.CodeNo = NewebpayRecive.Result.CodeNo;                        // 超商編號
                ViewBag.DueDate = DateTime.Now.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");  // 付款期限


                /*** 將Newebpay編號、付款方式、超商編號、繳費期限寫入資料庫 ***/
                viprecord.Tradenumber = NewebpayRecive.Result.TradeNo;
                viprecord.Paymenttype = NewebpayRecive.Result.PaymentType;
                viprecord.Paymentnumber = NewebpayRecive.Result.CodeNo;
                viprecord.Duedate = DateTime.Now.AddDays(7);
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Paymentnumber", "Duedate" });
                viprecordService.SaveChanges();
            }
            else if(NewebpayRecive.Result.PaymentType == "VACC")
            {
                string DepositNumber = NewebpayRecive.Result.MerchantOrderNo;          // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                                          // 付款方式
                ViewBag.Amt = NewebpayRecive.Result.Amt;                                                       // 付款金額
                ViewBag.Day = viprecord.Day;                                                                // 購買天數
                ViewBag.CodeNo = NewebpayRecive.Result.CodeNo;                                                 // 金融繳費代碼
                ViewBag.BankCode = NewebpayRecive.Result.BankCode;                                             // 銀行數字代碼
                ViewBag.DueDate = DateTime.Now.AddDays(7).ToString("yyyy/MM/dd HH:mm:ss");                  //繳費期限

                /*** 將Newebpay編號、付款方式、金融機構代碼、金融繳費代碼、繳費期限寫入資料庫 ****/
                viprecord.Tradenumber = NewebpayRecive.Result.TradeNo;
                viprecord.Paymenttype = NewebpayRecive.Result.PaymentType;
                viprecord.Bankcode = NewebpayRecive.Result.Bankcode;
                viprecord.Virtualaccount = NewebpayRecive.Result.CodeNo;
                viprecord.Duedate = DateTime.Now.AddDays(7);
                viprecordService.SpecificUpdate(viprecord, new string[] { "Tradenumber", "Paymenttype", "Bankcode", "Virtualaccount", "Duedate" });
                viprecordService.SaveChanges();
            }
            else if(NewebpayRecive.Result.PaymentType == "CREDIT")
            {
                string DepositNumber = NewebpayRecive.Result.MerchantOrderNo;          // 取得儲值編號
                Viprecord viprecord = viprecordService.Get().Where(a => a.Depositnumber == DepositNumber).FirstOrDefault();
                ViewBag.Payway = viprecord.Payway;                                                          // 付款方式
                ViewBag.Status = NewebpayRecive.Message;                                                    // 付款狀態
                ViewBag.Amt = NewebpayRecive.Result.Amt;                                                    // 付款金額
                ViewBag.Day = viprecord.Day;                                                                // 購買天數
                ViewBag.OrderDate = NewebpayRecive.Result.PayTime;                                          // 付款日期
                /*** 判斷是否授權成功 ***/
                if(NewebpayRecive.Message == "授權成功")
                {
                    /*** 將付款方式、Newebpay交易編號、開始時間、截止時間 ***/
                    Guid Memberid = Guid.Parse((Session["Memberid"]).ToString());
                    Viprecord old_data = viprecordService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(x => x.Enddate).FirstOrDefault();
                    viprecord.Tradenumber = NewebpayRecive.Result.TradNo;
                    viprecord.Paymenttype = NewebpayRecive.Result.PaymentType;
                    viprecord.Status = 2;
                    if (old_data != null)
                    {
                        if (old_data.Enddate < DateTime.Now)
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
            }
            return View();
        }
    }
}