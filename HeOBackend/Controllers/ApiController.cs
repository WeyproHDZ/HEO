using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeO.Models;
using HeO.Service;
using System.Data.Entity;
using System.Net;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Web.Helpers;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace HeOBackend.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private MemberloginrecordService memberloginrecordService;
        private SettingService settingService;
        private OrderService orderService;
        private OrderfacebooklistService orderfacebooklistService;
        private FeedbackproductService feedbackproductService;
        private ServicelogService servicelogService;
        private UseragentService useragentService;
        public ApiController()
        {
            orderService = new OrderService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberloginrecordService = new MemberloginrecordService();
            settingService = new SettingService();
            orderfacebooklistService = new OrderfacebooklistService();
            feedbackproductService = new FeedbackproductService();
            servicelogService = new ServicelogService();
            useragentService = new UseragentService();
        }

        int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800;      //目前時間總秒數
        [HttpGet]
        /*** Service要訂單 ***/
        public JsonResult GetOrder(string Id)
        {
            if (Id == "heo_order")
            {
                Order order = orderService.Get().OrderBy(o => o.Createdate).Where(a => a.OrderStatus == 1).FirstOrDefault();
                if (order == null)
                {
                    order = orderService.Get().OrderBy(o => o.Createdate).Where(a => a.OrderStatus == 0).FirstOrDefault();
                }
                if (order != null)
                {
                    // 將訂單狀態改為進行中
                    order.OrderStatus = 1;
                    order.Service = order.Service;
                    orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                    orderService.SaveChanges();
                    /*** 傳送訂單 ***/
                    string[] OrderArray = new string[6];
                    OrderArray[0] = order.Ordernumber;
                    OrderArray[1] = order.Remains.ToString();
                    OrderArray[2] = order.Url;
                    OrderArray[3] = order.Service;
                    OrderArray[4] = order.Message;
                    OrderArray[5] = (Now + 3600).ToString();

                    return this.Json(OrderArray, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return this.Json("目前沒有訂單", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                string status = "error";
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        /**** 更新訂單 ***/
        public JsonResult UpdateOrder(string Id, string Ordernumber, string status = "failed")
        {
            if (Ordernumber != null && Id == "heo_order" && status == "success")
            {
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();
                // 將訂單狀態改為完成中
                order.OrderStatus = 2;
                orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                orderService.SaveChanges();
                /*** 判斷訂單是否是hdz餵來的 ***/
                if (Ordernumber.Contains("Hdz"))
                {
                    /*** 回傳訂單編號及成功字眼到hdz ***/
                    string url = "https://www.hdzbulk.com/index.php/backend/api_connect/get_heo_status/heo_order?Ordernumber=" + Ordernumber + "&&status=success";
                    WebRequest myReq = WebRequest.Create(url);
                    myReq.Method = "GET";
                    myReq.ContentType = "application/json; charset=UTF-8";
                    UTF8Encoding enc = new UTF8Encoding();
                    myReq.Headers.Remove("auth-token");
                    WebResponse wr = myReq.GetResponse();
                    Stream receiveStream = wr.GetResponseStream();
                }
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else if(Ordernumber != null && Id == "heo_order" && status == "failed")
            {
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();
                // 將訂單狀態改為失敗
                order.OrderStatus = 3;
                orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                orderService.SaveChanges();
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        /*** Service要帳密 ***/
        public JsonResult GetAccount(string Id, int number, string Ordernumber)
        {
            if (Id == "heo_order")
            {
                Setting setting = settingService.Get().FirstOrDefault();
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();                                                     // 撈訂單的詳細資料
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(a => a.Feedbackproductname.Contains(order.Service)).FirstOrDefault();      // 撈訂單產品的詳細資料
                IEnumerable<Orderfaceooklist> orderfacebookList = orderfacebooklistService.Get().Where(a => a.Order.Ordernumber == Ordernumber);            // 撈訂單之前的按讚名單紀錄
                IEnumerable<Order> old_order = orderService.Get().Where(x => x.Ordernumber != order.Ordernumber).Where(c => c.Service == feedbackproduct.Feedbackproductname).Where(a => a.Url == order.Url);   // 撈所有訂單裡網址為訂單的資料
                List<get_old_member> MemberList = new List<get_old_member>();
                /**** 判斷訂單是不是HEO內部的單 ***/
                if (order.Ordernumber.Contains("heo"))
                {
                    /*** 先將自己給排除掉 ****/
                    MemberList.Add(
                        new get_old_member
                        {
                            Memberid = order.Members.Memberid
                        });
                }               
                /**** 將這張單的按讚列表的人排除掉 *****/
                if(orderfacebookList.Count() > 0)
                {
                    foreach(Orderfaceooklist thisorderfacebookList in orderfacebookList)
                    {
                        MemberList.Add(
                            new get_old_member
                            {
                                Memberid = Guid.Parse(thisorderfacebookList.Memberid.ToString())
                            }
                        );
                    }
                }
                /*** 將同網址的訂單的按站讚列表的人排除掉 ****/
                if (old_order.Count() > 0)
                {
                    foreach (Order thisold_order in old_order)
                    {
                        IEnumerable<Orderfaceooklist> oldorderfacebooklist = orderfacebooklistService.Get().Where(a => a.Orderid == thisold_order.Orderid);
                        foreach (Orderfaceooklist thisoldorderfaceooklist in oldorderfacebooklist)
                        {
                            MemberList.Add(
                                new get_old_member
                                {
                                    Memberid = Guid.Parse(thisoldorderfaceooklist.Memberid.ToString())
                                }
                            );
                        }
                    }
                }

                Guid VipLevelid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;   // VIP層級的ID

                if (order.Ordernumber.Contains("heo"))
                {

                    /*** HEO內部下的訂單 ***/                    
                    List<get_member> AccountList = new List<get_member>();
                    /*** 先撈前台登入、仿前台登入的使用者 ***/
                    IEnumerable<Members> members = membersService.Get().Where(s => s.Is_import == 0 || s.Is_import == 2).Where(c => c.Levelid != VipLevelid).Where(x => x.Lastdate <= Now).Where(a => a.Logindate >= Now).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status == 1).OrderBy(x => x.Lastdate);       // 撈層級非VIP的帳號詳細資料
                    if (members != null)
                    {                        
                        foreach (Members thismembers in members)
                        {
                            bool used = false;
                            int loop;
                            for (loop = 0; loop < MemberList.Count(); loop++)
                            {                                        
                                if (thismembers.Memberid.Equals(MemberList[loop].Memberid) == true)
                                {
                                    used = true;
                                }
                                if (used == false)
                                {
                                    AccountList.Add(
                                        new get_member
                                        {
                                            memberid = thismembers.Memberid,
                                            account = thismembers.Account,
                                            password = thismembers.Password,
                                            useragent_phone = thismembers.Useragent_phone,
                                            facebookcookie = thismembers.Facebookcookie,
                                            duedate = (Now + 3600)
                                        }
                                    );
                                    
                                }
                            }
                            used = false;
                        }
                        
                        /*** 可用人數小於該訂單所需人數 ***/
                        if (AccountList.Count() < number)
                        {                            
                            bool used = false;
                            //int remains = number - AccountList.Count();
                            /**** 假設前台登入的都冷卻中，就拿後台匯入的機器人 ****/
                            members = membersService.Get().Where(s => s.Is_import == 1).Where(x => x.Lastdate <= Now).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status != 2).OrderBy(x => x.Lastdate);
                            foreach (Members thismembers in members)
                            {
                                int loop;
                                for (loop = 0; loop < MemberList.Count(); loop++)
                                {                                    
                                    if (thismembers.Memberid.Equals(MemberList[loop].Memberid) == true)
                                    {                                        
                                        used = true;
                                    }

                                    if (used == false)
                                    {
                                        AccountList.Add(
                                            new get_member
                                            {
                                                memberid = thismembers.Memberid,
                                                account = thismembers.Account,
                                                password = thismembers.Password,
                                                useragent_phone = thismembers.Useragent_phone,
                                                facebookcookie = thismembers.Facebookcookie,
                                                duedate = (Now + 3600)
                                            }
                                        );                                        
                                    }
                                    used = false;
                                }
                            }
                            if (AccountList.Count() < number)
                            {
                                return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (get_member entity in AccountList.Take(number))
                                {
                                    /*** 將此會員更新下次互惠時間 ****/
                                    Members member = membersService.GetByID(entity.memberid);
                                    member.Lastdate = Now + (int)setting.Time;
                                    membersService.SpecificUpdate(member, new string[] { "Lastdate" });
                                }
                                membersService.SaveChanges();
                            }
                        }
                        foreach (get_member entity in AccountList.Take(number))
                        {
                            /*** 將此會員更新下次互惠時間 ****/
                            Members member = membersService.GetByID(entity.memberid);
                            member.Lastdate = Now + (int)setting.Time;
                            membersService.SpecificUpdate(member, new string[] { "Lastdate" });
                        }
                        membersService.SaveChanges();
                        return this.Json(AccountList.Take(number), JsonRequestBehavior.AllowGet);
                    }
                    else
                    {                        
                        return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    /*** HDZ餵來的訂單 ***/
                    if (order.Isreal == true)
                    {
                        /*** 此筆訂單需要真人帳號 ****/
                        IEnumerable<Members> members = membersService.Get().Where(c => c.Isreal == true).Where(x => x.Lastdate <= Now).Where(w => w.Memberloginrecord.OrderByDescending(e => e.Createdate).FirstOrDefault().Status == 1).Where(s => s.Memberauthorization.Where(q => q.Feedbackproductid == feedbackproduct.Feedbackproductid).FirstOrDefault().Checked == true).OrderBy(x => x.Lastdate);
                        if (members != null)
                        {
                            List<get_member> AccountList = new List<get_member>();
                            bool used_accoount = false;
                            foreach (Members thismembers in members)
                            {
                                int loop;
                                if (MemberList.Count() > 0)
                                {
                                    for (loop = 0; loop < MemberList.Count(); loop++)
                                    {
                                        if ((thismembers.Memberid.Equals(MemberList[loop].Memberid)) == true)
                                        {
                                            used_accoount = true;
                                        }
                                    }
                                }

                                if (!used_accoount)
                                {
                                    AccountList.Add(
                                        new get_member
                                        {
                                            memberid = thismembers.Memberid,
                                            account = thismembers.Account,
                                            password = thismembers.Password,
                                            useragent_phone = thismembers.Useragent_phone,
                                            facebookcookie = thismembers.Facebookcookie,
                                            duedate = (Now+3600)
                                        }
                                    );
                                }
                                used_accoount = false;
                            }

                            /*** 可用人數小於該訂單所需人數 ***/
                            if (AccountList.Count < number)
                            {
                                return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (get_member entity in AccountList.Take(number))
                                {
                                    /*** 將此會員更新下次互惠時間 ****/
                                    Members member = membersService.GetByID(entity.memberid);
                                    member.Lastdate = Now  + (int)setting.Time;
                                    membersService.SpecificUpdate(member, new string[] { "Lastdate" });
                                }
                            }

                            membersService.SaveChanges();
                            return this.Json(AccountList.Take(number), JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        IEnumerable<Members> members = membersService.Get().Where(x => x.Lastdate <= Now).Where(b => b.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status != 2).Where(s => s.Memberauthorization.Where(q => q.Feedbackproductid == feedbackproduct.Feedbackproductid).FirstOrDefault().Checked == true).OrderBy(x => x.Lastdate);
                        if (members != null)
                        {
                            List<get_member> AccountList = new List<get_member>();
                            bool used_accoount = false;
                            foreach (Members thismembers in members)
                            {
                                int loop;
                                if (MemberList.Count() > 0)
                                {
                                    for (loop = 0; loop < MemberList.Count(); loop++)
                                    {
                                        if ((thismembers.Memberid.Equals(MemberList[loop].Memberid)) == true)
                                        {
                                            used_accoount = true;
                                        }
                                    }
                                }

                                if (!used_accoount)
                                {
                                    AccountList.Add(
                                        new get_member
                                        {
                                            memberid = thismembers.Memberid,
                                            account = thismembers.Account,
                                            password = thismembers.Password,
                                            useragent_phone = thismembers.Useragent_phone,
                                            facebookcookie = thismembers.Facebookcookie,
                                            duedate = (Now+3600)
                                        }
                                    );
                                }
                                used_accoount = false;
                            }

                            /*** 可用人數小於該訂單所需人數 ***/
                            if (AccountList.Count < number)
                            {
                                return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                            }
                            else
                            {
                                foreach (get_member entity in AccountList.Take(number))
                                {
                                    /*** 將此會員更新下次互惠時間 ****/
                                    Members member = membersService.GetByID(entity.memberid);
                                    member.Lastdate = Now + (int)setting.Time;
                                    membersService.SpecificUpdate(member, new string[] { "Lastdate" });
                                }
                            }

                            membersService.SaveChanges();
                            return this.Json(AccountList.Take(number), JsonRequestBehavior.AllowGet);

                        }
                        else
                        {
                            return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                        }
                    }
                }
            }
            else
            {
                string status = "Error";
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        /*** 更新會員互惠列表 ***/
        public JsonResult UpdateAccount(string Id, string Ordernumber, string Memberid, string FacebookCookie, int AccountStatus, string Facebookid)
        {
            if (Id == "heo_order")
            {
                IEnumerable<Memberlevel> memberlevel = memberlevelService.Get().Where(a => a.Isenable == 1);                // 撈除了真人以外的層級
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();                 // 該訂單的詳細資料
                Members member = membersService.GetByID(Guid.Parse(Memberid));                                              // 該會員的詳細資料
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(a => a.Feedbackproductname.Contains(order.Service)).FirstOrDefault();          // 該訂單之產品資料
                if (AccountStatus == 0 || AccountStatus == 1)         // 帳號需驗證、帳密錯誤、更改密碼
                {
                    /**** 將登入失敗寫入資料庫 ****/
                    Memberloginrecord Memberloginrecord = new Memberloginrecord();
                    Memberloginrecord.Memberid = Guid.Parse(Memberid);
                    Memberloginrecord.Status = 2;
                    Memberloginrecord.Createdate = DateTime.Now;
                    memberloginrecordService.Create(Memberloginrecord);
                    memberloginrecordService.SaveChanges();
                    /**** 寫入TXT檔 *****/
                    using (StreamWriter sw = new StreamWriter(@"C:\Users\wadmin\Desktop\HEO_order.txt", true))
                    {
                        sw.Write("HEO訂單問題回報 會員帳號:" + member.Account + "登入有問題(密碼更改or帳密錯誤)");
                        sw.Write(Environment.NewLine);
                        sw.Write(DateTime.Now);
                        sw.Write(Environment.NewLine);
                    }
                }
                else if(AccountStatus == 2)     // 按讚成功
                {
                    /*** 改訂單剩餘人數 ***/
                    order.Remains -= 1;
                    orderService.SpecificUpdate(order, new string[] { "Remains" });
                    orderService.SaveChanges();
                    /**** 將登入成功寫入資料庫 ****/
                    Memberloginrecord Memberloginrecord = new Memberloginrecord();
                    Memberloginrecord.Memberid = Guid.Parse(Memberid);
                    Memberloginrecord.Status = 1;
                    Memberloginrecord.Createdate = DateTime.Now;
                    memberloginrecordService.Create(Memberloginrecord);
                    memberloginrecordService.SaveChanges();
                }
                else    // 找不到讚的位置
                {
                    /***** 寄信給我 ****/
                    order.OrderStatus = 3;      //訂單改為失敗
                    orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                    orderService.SaveChanges();
                    /**** 寫入TXT檔 *****/
                    using (StreamWriter sw = new StreamWriter(@"C:\Users\wadmin\Desktop\HEO_order.txt", true))
                    {
                        sw.Write("HEO訂單問題回報 訂單編號:" + order.Ordernumber + "有問題，" + member.Account +"(找不到讚的位置)");
                        sw.Write(Environment.NewLine);
                        sw.Write(DateTime.Now);
                        sw.Write(Environment.NewLine);
                    }
                    
                }
                if (order.Ordernumber.Contains("heo"))
                {
                    /*** HEO內部下單 ***/
                    /*** 更新會員的FacebookId ***/
                    member.Facebookid = Facebookid;
                    /*** 更新會員的Cookie *****/
                    member.Facebookcookie = FacebookCookie;
                    membersService.SpecificUpdate(member, new string[] { "Facebookcookie", "Facebookid" });
                    /*** 將會員寫到該訂單的互惠會員列表 ***/
                    Orderfaceooklist orderfacebooklist = new Orderfaceooklist();
                    orderfacebooklist.Memberid = member.Memberid;
                    orderfacebooklist.Feedbackproductid = feedbackproduct.Feedbackproductid;
                    orderfacebooklist.Facebookaccount = member.Account;
                    orderfacebooklist.Orderid = order.Orderid;
                    orderfacebooklist.Createdate = DateTime.Now;
                    orderfacebooklist.Updatedate = DateTime.Now;
                    //orderfacebooklistService.Create(orderfacebooklist);
                    member.Orderfaceooklist.Add(orderfacebooklist);
                    membersService.SaveChanges();
                    return this.Json("Success");
                }
                else
                {
                    /**** HDZ餵來的訂單 ****/
                    /*** 更新訂單成本及判斷該會員的層級，並且撥對應的回饋金給該會員 ****/
                    if (member.Is_import == 0)           // 判斷該會員非後台匯入的會員
                    {
                        if (member.Isreal == true)          // 判斷該會員是否為真人
                        {
                            order.Cost += 1.0 * Convert.ToDouble(feedbackproduct.Feedbackdetail.FirstOrDefault(a => a.Memberlevel.Levelname == "真人").Money);
                            member.Feedbackmoney += order.Cost;
                        }
                        else
                        {
                            foreach (Memberlevel level in memberlevel)
                            {
                                if (member.Memberlevel.Levelname == level.Levelname)
                                {
                                    order.Cost += 1.0 * Convert.ToDouble(feedbackproduct.Feedbackdetail.FirstOrDefault(a => a.Memberlevel.Levelname == level.Levelname).Money);
                                }
                            }
                        }
                    }

                    orderService.SpecificUpdate(order, new string[] { "Cost" });
                    /*** 將會員寫到該訂單的互惠會員列表 ***/
                    Orderfaceooklist orderfacebooklist = new Orderfaceooklist();
                    orderfacebooklist.Memberid = member.Memberid;
                    orderfacebooklist.Feedbackproductid = feedbackproduct.Feedbackproductid;
                    orderfacebooklist.Facebookaccount = member.Account;
                    orderfacebooklist.Orderid = order.Orderid;
                    orderfacebooklist.Createdate = DateTime.Now;
                    orderfacebooklist.Updatedate = DateTime.Now;
                    order.Orderfaceooklist.Add(orderfacebooklist);
                    orderService.SaveChanges();
                    // orderfacebooklistService.Create(orderfacebooklist);
                    /*** 判斷該會員的層級，並且撥對應的回饋金給該會員 ***/
                    if (member.Is_import == 0)           // 判斷該會員非後台匯入的會員
                    {
                        if (member.Isreal == true)          // 判斷該會員是否為真人
                        {
                            member.Feedbackmoney += Convert.ToInt32(feedbackproduct.Feedbackdetail.FirstOrDefault(a => a.Memberlevel.Levelname == "真人").Money);
                        }
                        else
                        {
                            member.Feedbackmoney += Convert.ToInt32(feedbackproduct.Feedbackdetail.FirstOrDefault(a => a.Levelid == member.Levelid).Money);
                        }
                    }
                    /*** 更新該會員的FacebookId ***/
                    member.Facebookid = Facebookid;
                    /*** 更新該會員的Cookie *****/
                    member.Facebookcookie = FacebookCookie;
                    membersService.SpecificUpdate(member, new string[] { "Facebookcookie", "Feedbackmoney", "Facebookid" });
                    membersService.SaveChanges();
                    return this.Json("Success");
                }
            }
            else
            {
                string status = "Error";
                return this.Json(status);
            }
        }

        /*** 查看訂單狀態 ****/
        [HttpGet]
        public JsonResult CheckOrderStatus(string Id, string OrderNumber)
        {
            if(Id == "heo_order")
            {
                Order order = orderService.Get().Where(a => a.Ordernumber == OrderNumber).FirstOrDefault();
                return this.Json(order.OrderStatus, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        /*** 寫入Log **/
        public JsonResult Service_log(string Id, string text, string time)
        {
            if (Id == "heo_order")
            {
                Servicelog servicelog = new Servicelog();
                servicelog.Logs = text;
                servicelog.Createdate = time;
                servicelogService.Create(servicelog);
                servicelogService.SaveChanges();
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        /*** heo向hdz要訂單 ***/
        public JsonResult GetHdzOrder(string[] heo_array)
        {
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            if (heo_array[0] == "hdz_order")
            {

                Order order = new Order();
                order.Orderid = Guid.NewGuid();
                order.Ordernumber = heo_array[1];
                order.Url = heo_array[2].Replace(" ", "");
                order.Isreal = heo_array[3] == "true" ? true : false;

                foreach (Feedbackproduct feedbackproductlist in feedbackproduct)
                {
                    if (heo_array[4].Contains(feedbackproductlist.Feedbackproductname))
                    {
                        order.Service = feedbackproductlist.Feedbackproductname;
                        order.Message = heo_array[6];
                    }
                }
                order.Count = Convert.ToInt32(heo_array[5]);
                order.Remains = Convert.ToInt32(heo_array[5]);
                order.Createdate = DateTime.Now;
                order.Updatedate = DateTime.Now;
                orderService.Create(order);
                orderService.SaveChanges();
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(heo_array, JsonRequestBehavior.AllowGet);
            }
        }

        /*** 目前在縣人數 ****/
        [HttpGet]
        public JsonResult Now_Members(string Id)
        {
            if(Id == "F0lqG")
            {
                #region --目前登入人數--
                Setting setting = settingService.Get().FirstOrDefault();
                Guid Vipid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;
                int number = membersService.Get().Where(x => x.Levelid != Vipid).Where(a => a.Is_import == 0).Where(a => a.Logindate >= Now).Count();
                return this.Json(number, JsonRequestBehavior.AllowGet);
                #endregion
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        [HttpGet]
        public JsonResult hMeyoIyPa()
        {
            #region --只選出a-z、A-Z、小老鼠、小數點，其餘捨去--
            //string s = @"=+7 (931) 533-2433";
            //string r = Regex.Replace(s, @"[^a-z||A-Z||@||.||1-9]", "");
            //return this.Json(r, JsonRequestBehavior.AllowGet);
            #endregion
            #region --顯示真實IP--
            //string ipaddress;
            //ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (ipaddress == "" || ipaddress == null)
            //{
            //    ipaddress = Request.ServerVariables["REMOTE_ADDR"];
            //}
            //return this.Json(ipaddress, JsonRequestBehavior.AllowGet);
            #endregion
            #region --取得網頁port號--
            //int url;
            //url = Request.Url.Port;
            //return this.Json(url, JsonRequestBehavior.AllowGet);
            #endregion
            #region --取得電腦名稱--
            //string name = Dns.GetHostName();
            //return this.Json(name, JsonRequestBehavior.AllowGet);
            #endregion
            #region--取得MAC Address--
            //NetworkInterface[] nics = NetworkInterface.GetAllNetworkInterfaces();
            //string[] mac = new string[1];
            //List<string> macList = new List<string>();
            //foreach (var nic in nics)
            //{
            //    if (nic.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
            //    {
            //        macList.Add(nic.GetPhysicalAddress().ToString());
            //    }
            //}
            //return this.Json(macList[0], JsonRequestBehavior.AllowGet);
            #endregion
            #region --Useragent列表--
            //int useragent_phone = useragentService.Get().Where(a => a.Isweb == 1).Count();
            //Random rnd = new Random();
            //int rnd_useragent = rnd.Next(1, useragent_phone);
            //Useragent useragent = useragentService.Get().Where(a => a.Id == rnd_useragent).FirstOrDefault();

            //return this.Json(useragent.User_agent+ "," + rnd_useragent + "," + useragent_phone, JsonRequestBehavior.AllowGet);
            #endregion
            #region --Self_hosted(Post)--
            //var httpWebRequest = (HttpWebRequest)WebRequest.Create("http://heofrontend.4webdemo.com:8080/Check/Login");
            //httpWebRequest.ContentType = "application/json";
            //httpWebRequest.Method = "POST";

            //using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            //{
            //    string json = new JavaScriptSerializer().Serialize(new
            //    {
            //        user = "Foo",
            //        password = "Baz"
            //    });

            //    streamWriter.Write(json);
            //}

            //var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            //string result = "";
            //using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            //{
            //    result = streamReader.ReadToEnd();

            //}
            //return this.Json(result, JsonRequestBehavior.AllowGet);
            #endregion
            #region --目前登入人數--
            //Setting setting = settingService.Get().FirstOrDefault();
            //Guid Vipid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;
            //int number = membersService.Get().Where(x => x.Levelid != Vipid).Where(a => a.Logindate >= Now).Count();
            //return this.Json(Now + "." + (int)setting.Time, JsonRequestBehavior.AllowGet);
            #endregion
            string status = "100041065965912";            
            if (Regex.IsMatch(status, "^[0-9]*$"))
            {
                return this.Json(1, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json(0, JsonRequestBehavior.AllowGet);
            }
            //return this.Json(member.Count(), JsonRequestBehavior.AllowGet);
           
        }

        #region --新增會員--
        /**** 新增會員 ****/
        [HttpPost]
        public JsonResult HEO_InsertMembers(string Id, string Account, string Password, string Name, string Facebookid, string Cookie)
        {
            if (Id == "HEOInsertMembers")
            {
                int useragent_count = useragentService.Get().Where(a => a.Isweb == 1).Count();
                Random crandom = new Random();
                int rand = crandom.Next(0, useragent_count - 1);
                Useragent[] useragent = useragentService.Get().Where(a => a.Isweb == 1).ToArray();
                Members member = new Members();
                member.Isenable = 1;
                member.Is_import = 1;
                member.Memberid = Guid.NewGuid();
                member.Levelid = Guid.Parse("0db21b54-35a7-400b-a8ea-e9c4c2879609");
                member.Account = Account;
                member.Password = Password;
                member.Name = Name;
                member.Facebookid = Facebookid;
                member.Facebookcookie = Cookie;
                member.Useragent_phone = useragent[rand].User_agent;
                member.Logindate = 99999999999;
                member.Lastdate = Now;
                member.Createdate = DateTime.Now;
                member.Updatedate = DateTime.Now;
                /**** 寫入登入列表裡 ****/
                Memberloginrecord memberloginrecord = new Memberloginrecord();
                memberloginrecord.Memberid = member.Memberid;
                memberloginrecord.Status = 1;
                memberloginrecord.Createdate = DateTime.Now;
                member.Memberloginrecord.Add(memberloginrecord);
                /**** 開產品權限 ****/
                IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get().ToList();
                /**** 產品授權功能，預設都為true 【false : 未授權 , true: 已授權】 ****/
                foreach (Feedbackproduct productList in feedbackproduct)
                {
                    Memberauthorization memberauthorization = new Memberauthorization();
                    memberauthorization.Id = Guid.NewGuid();
                    memberauthorization.Memberid = member.Memberid;
                    memberauthorization.Feedbackproductid = productList.Feedbackproductid;
                    memberauthorization.Checked = true;
                    member.Memberauthorization.Add(memberauthorization);
                }
                membersService.Create(member);
                membersService.SaveChanges();
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
        #endregion

    }
    public class get_member
    {
        public Guid memberid { get; set; }
        public string account { get; set; }
        public string password { get; set; }
        public string useragent_phone { get; set; }
        public string facebookcookie { get; set; }
        public int duedate { get; set; }
    }

    public class get_old_member
    {
        public Guid Memberid { get; set; }
    }

    public class json_format
    {
        public string name { get; set; }
        public string value { get; set; }
        public string domain { get; set; }
        public string path { get; set; }
        public int expiry { get; set; }
    }
}