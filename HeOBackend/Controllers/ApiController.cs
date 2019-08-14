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
            settingService = new SettingService();
            orderfacebooklistService = new OrderfacebooklistService();
            feedbackproductService = new FeedbackproductService();
            servicelogService = new ServicelogService();
            useragentService = new UseragentService();
        }

        [HttpGet]
        /*** Service要訂單 ***/
        public JsonResult GetOrder(string Id)
        {
            if (Id == "heo_order")
            {
                Order order = orderService.Get().OrderBy(o => o.Createdate).Where(a => a.OrderStatus == 1).FirstOrDefault();
                if(order == null)
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
                    string[] OrderArray = new string[5];
                    OrderArray[0] = order.Ordernumber;
                    OrderArray[1] = order.Remains.ToString();
                    OrderArray[2] = order.Url;
                    OrderArray[3] = order.Service;
                    OrderArray[4] = order.Message;

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
            if(Ordernumber != null && Id == "heo_order" && status == "success")
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
                    string url = "http://hdz.4webdemo.com/index.php/backend/api_connect/get_heo_status/heo_order?Ordernumber=" + Ordernumber+"&&status=success";
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
                int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;                                                                          // 目前時間的總秒數
                Setting setting = settingService.Get().FirstOrDefault();
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();                                                     // 撈此訂單的詳細資料
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(a => a.Feedbackproductname.Contains(order.Service)).FirstOrDefault();      // 撈此訂單所需之產品的詳細資料                
                IEnumerable<Order> old_order = orderService.Get().Where(x => x.Ordernumber != order.Ordernumber).Where(c => c.Service == feedbackproduct.Feedbackproductname).Where(a => a.Url == order.Url);                // 撈所有訂單裡網址為此訂單的資料
                List<get_old_member> MemberList = new List<get_old_member>();
                                
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
                                    account = thisoldorderfaceooklist.Facebookaccount
                                }
                            );
                        }
                    }
                }
                Guid VipLevelid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;                                           // VIP層級的ID
                
                if (order.Ordernumber.Contains("heo"))
                {                    
                    /*** HEO內部下的訂單 ***/
                    IEnumerable<Members> members = membersService.Get().Where(c => c.Levelid != VipLevelid).Where(x => x.Lastdate <= Now).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status == 1);       // 撈層級非VIP的帳號詳細資料
                    if (members != null)
                    {
                        List<get_member> AccountList = new List<get_member>();
                        bool used_accoount = false;
                        foreach (Members thismembers in members)
                        {
                            int loop;
                            if(MemberList.Count() > 0)
                            {
                                for(loop = 0;loop < MemberList.Count(); loop++)
                                {
                                    if ((thismembers.Account.Equals(MemberList[loop].account)))
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
                                        facebookcookie = thismembers.Facebookcookie
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
                            foreach(get_member entity in AccountList.Take(number))
                            {
                                /*** 將此會員更新下次互惠時間 ****/
                                Members member = membersService.GetByID(entity.memberid);
                                member.Lastdate += (int)setting.Time;
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
                    /*** HDZ餵來的訂單 ***/
                    if (order.Isreal == true)
                    {
                        /*** 此筆訂單需要真人帳號 ****/
                        IEnumerable<Members> members = membersService.Get().Where(c => c.Isreal == true).Where(x => x.Lastdate <= Now).Where(w => w.Memberloginrecord.OrderByDescending(e => e.Createdate).FirstOrDefault().Status == 1).Where(s => s.Memberauthorization.Where(q => q.Feedbackproductid == feedbackproduct.Feedbackproductid).FirstOrDefault().Checked == true);
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
                                        if ((thismembers.Account.Equals(MemberList[loop].account)))
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
                                            facebookcookie = thismembers.Facebookcookie
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
                                    member.Lastdate += (int)setting.Time;
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
                        IEnumerable<Members> members = membersService.Get().Where(x => x.Lastdate <= Now).Where(b => b.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1).Where(s => s.Memberauthorization.Where(q => q.Feedbackproductid == feedbackproduct.Feedbackproductid).FirstOrDefault().Checked == true);
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
                                        if ((thismembers.Account.Equals(MemberList[loop].account)))
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
                                            facebookcookie = thismembers.Facebookcookie
                                        }
                                    );
                                    /*** 將此會員更新下次互惠時間 ****/
                                    thismembers.Lastdate += (int)setting.Time;
                                    membersService.SpecificUpdate(thismembers, new string[] { "Lastdate" });

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
                                    member.Lastdate += (int)setting.Time;
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
        public JsonResult UpdateAccount(string Id, string Ordernumber, string Memberid, string FacebookCookie)
        {
            if(Id == "heo_order")
            {
                IEnumerable<Memberlevel> memberlevel = memberlevelService.Get().Where(a => a.Isenable == 1);                // 撈除了真人以外的層級
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();                 // 該訂單的詳細資料
                Members member = membersService.GetByID(Guid.Parse(Memberid));                                              // 該會員的詳細資料
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(a => a.Feedbackproductname.Contains(order.Service)).FirstOrDefault();          // 該訂單之產品資料
                /*** 改訂單剩餘人數 ***/
                order.Remains -= 1;

                orderService.SpecificUpdate(order, new string[] { "Remains" });
                orderService.SaveChanges();
                if (order.Ordernumber.Contains("heo"))
                {
                    /*** HEO內部下單 ***/
                    /*** 判斷該會員的資料庫欄位裡的Cookie有沒有資料 *****/
                    if(member.Facebookcookie == null)
                    {
                        FacebookCookie = FacebookCookie.Replace("'", @"\");     // 將\ 取代成 '
                        membersService.SpecificUpdate(member, new string[] { "Facebookcookie" });
                    }
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
                    if(member.Is_import == false)           // 判斷該會員非後台匯入的會員
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
                    if(member.Is_import == false)           // 判斷該會員非後台匯入的會員
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

                    /*** 判斷該會員的資料庫欄位裡的Cookie有沒有資料 *****/
                    if (member.Facebookcookie == null)
                    {
                        FacebookCookie = FacebookCookie.Replace("'", @"\");     // 將\ 取代成 '
                        membersService.SpecificUpdate(member, new string[] { "Facebookcookie" });
                    }
                    membersService.SpecificUpdate(member, new string[] { "Feedbackmoney" });
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
                order.Url = heo_array[2];
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

        [HttpGet]
        public JsonResult Now_members(string Id)
        {
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;      // 目前時間的總秒數
            //int members = membersService.Get().Where(x => x.Lastdate <= Now).Count();
            IEnumerable<Members> members = membersService.Get().Where(x => x.Lastdate <= Now).Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1);
            IEnumerable<Memberlevel> memberlevel = memberlevelService.Get();
            int[] members_count = new int[3];
            string[] members_levelname = new string[3];
            int i = 0;
            if (Id == "heo_members")
            {
                foreach (Memberlevel memberlevellist in memberlevel)
                {
                    members_levelname[i] = memberlevellist.Levelname;
                    foreach (Members memberlist in members)
                    {
                        if (memberlevellist.Levelid == memberlist.Levelid)
                        {
                            members_count[i]++;
                        }
                    }
                    i++;
                }
                return this.Json(members_levelname[0] + members_count[0] + "," + members_levelname[1] + members_count[1] + "," + members_levelname[2] + members_count[2], JsonRequestBehavior.AllowGet);
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
            int Now = ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800);         // 登入時間為現在時間的總秒數
            int number = membersService.Get().Where(a => a.Logindate >= Now).Count();
            return this.Json(Now, JsonRequestBehavior.AllowGet);
            #endregion

        }

        [HttpGet]
        public JsonResult cloudmanage(string Id)
        {
            if(Id == "ClOuD_MaNaGe")
            {
                int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;      // 目前時間的總秒數
                IEnumerable<Members> members = membersService.Get().Where(x => x.Lastdate <= Now).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status == 1);       // 撈所有能用的FB
                List<get_member> AccountList = new List<get_member>();
                AccountList.Add(
                    new get_member
                    {
                        memberid = Guid.Parse(("c6ed9156-c847-468d-a012-47949856fc26").ToString()),
                        account = "exorcist5859@livemail.tw",
                        password = "innocence5757",
                        useragent_phone = "Mozilla/5.0 (iPhone; CPU iPhone OS 7_0_4 like Mac OS X) AppleWebKit/537.51.1 (KHTML, like Gecko) Version/7.0 Mobile/11B554a Safari/9537.53",
                        facebookcookie = @"[{\Secure\:false,\IsHttpOnly\:false,\Name\:\datr\,\Value\:\7McmXQlXdEfjpeyQSFOMIks8\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2021-07-10T13:23:57+08:00\},{\Secure\:false,\IsHttpOnly\:false,\Name\:\sb\,\Value\:\7McmXdZHJzL8gLngTdF-d0d3\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2021-07-30T16:32:54+08:00\},{\Secure\:false,\IsHttpOnly\:false,\Name\:\c_user\,\Value\:\100002472553723\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2020-08-11T15:35:37+08:00\},{\Secure\:false,\IsHttpOnly\:false,\Name\:\xs\,\Value\:\26%3ApalMR0PMSXeAIQ%3A2%3A1564561973%3A7745%3A11316\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2020-08-11T15:35:37+08:00\},{\Secure\:false,\IsHttpOnly\:false,\Name\:\fr\,\Value\:\10jGuqfCI3pjOP4cm.AWW8UDkXIBfzMY5D6qCzLojMAnU.BdJsfs.F-.F0v.0.0.BdURbJ.AWUyPE99\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2020-08-11T15:35:36+08:00\},{\Secure\:true,\IsHttpOnly\:false,\Name\:\x-referer\,\Value\:\eyJyIjoiL2Vycm9yL2luZGV4LnBocD9lcnI9ZWMma2Vycj0xMzU3MDA0JmtlcnJfc3VtbWFyeT0lRTUlOTQlODklRTUlOTElODAlRUYlQkMlOEMlRTUlQTUlQkQlRTUlODMlOEYlRTYlOUMlODklRTYlOUQlQjElRTglQTUlQkYlRTUlODclQkElRTklOEMlQUYlRTQlQkElODYma2Vycl9kZXNjcmlwdGlvbj0lRTglQUIlOEIlRTUlOTglOTclRTglQTklQTYlRTklOTclOUMlRTklOTYlODklRTQlQjglQTYlRTklODclOEQlRTYlOTYlQjAlRTklOTYlOEIlRTUlOTUlOUYlRTQlQkQlQTAlRTclOUElODQlRTclODAlOEYlRTglQTYlQkQlRTUlOTklQTglRTglQTYlOTYlRTclQUElOTclRTMlODAlODIiLCJoIjoiL2Vycm9yL2luZGV4LnBocD9lcnI9ZWMma2Vycj0xMzU3MDA0JmtlcnJfc3VtbWFyeT0lRTUlOTQlODklRTUlOTElODAlRUYlQkMlOEMlRTUlQTUlQkQlRTUlODMlOEYlRTYlOUMlODklRTYlOUQlQjElRTglQTUlQkYlRTUlODclQkElRTklOEMlQUYlRTQlQkElODYma2Vycl9kZXNjcmlwdGlvbj0lRTglQUIlOEIlRTUlOTglOTclRTglQTklQTYlRTklOTclOUMlRTklOTYlODklRTQlQjglQTYlRTklODclOEQlRTYlOTYlQjAlRTklOTYlOEIlRTUlOTUlOUYlRTQlQkQlQTAlRTclOUElODQlRTclODAlOEYlRTglQTYlQkQlRTUlOTklQTglRTglQTYlOTYlRTclQUElOTclRTMlODAlODIiLCJzIjoibSJ9\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:null},{\Secure\:true,\IsHttpOnly\:false,\Name\:\wd\,\Value\:\1080x1920\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2019-08-19T15:39:32+08:00\},{\Secure\:true,\IsHttpOnly\:false,\Name\:\m_pixel_ratio\,\Value\:\1\,\Domain\:\.facebook.com\,\Path\:\/\,\Expiry\:\2019-08-19T15:39:32+08:00\}]"
                    }
                );
                return this.Json(AccountList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }
    }
    public class get_member
    {
        public Guid memberid { get; set; }
        public string account { get; set; }
        public string password { get; set; }
        public string useragent_phone { get; set; }
        public string facebookcookie { get; set; }
    }

    public class get_old_member
    {
        public string account { get; set; }
    }
}