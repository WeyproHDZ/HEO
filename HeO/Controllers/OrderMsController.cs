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
using System.Data.Entity;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNet.SignalR;
using Owin;
using Microsoft.Owin;

[assembly: OwinStartup(typeof(HeO.Controllers.Startup))]
namespace HeO.Controllers
{    
    public class OrderMsController : BaseController
    {
        private OrderService orderService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private MemberlevelcooldownService memberlevelcooldownService;
        private MemberblacklistService memberblacklistService;
        private SettingService settingService;
        
        int? Cooldowntime;
        public OrderMsController()
        {
            orderService = new OrderService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberlevelcooldownService = new MemberlevelcooldownService();
            memberblacklistService = new MemberblacklistService();
            settingService = new SettingService();
        }
        // GET: OrderMs
        [HttpGet]
        [CheckSession]
        public ActionResult Order()
        {
            Guid Memberid = Guid.Parse(System.Web.HttpContext.Current.Session["Memberid"].ToString());
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            Setting Setting = settingService.Get().FirstOrDefault();
            ViewBag.TotalNumber = membersService.Get().Count();         // 會員總人數
            ViewBag.Max = Setting.Max;
            ViewBag.Min = Setting.Min;

            /**** HTTP GET ****/
            string url = "http://www.heohelp.com:80/";
            WebRequest myReq = WebRequest.Create(url);
            myReq.Method = "GET";
            myReq.ContentType = "application/json; charset=UTF-8";
            UTF8Encoding enc = new UTF8Encoding();
            myReq.Headers.Remove("auth-token");
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            string[] status = content.Replace("\"", "").Split('#');
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Order(Order order)
        {
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800;      // 目前時間的總秒數
            //Guid Vipid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;    // VIPID

            int membersCount = membersService.Get().Where(x => x.Logindate >= Now).Where(b => b.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1).Count();    // 扣除Vip會員的所有可用人數
            if (order.Count > membersCount)
            {
                ViewBag.TotalNumber = membersService.Get().Count();         // 會員總人數
                Setting Setting = settingService.Get().FirstOrDefault();
                ViewBag.Max = Setting.Max;
                ViewBag.Min = Setting.Min;
                TempData["message"] = "數量錯誤，請重新下單!"+membersCount;
                return RedirectToAction("Order", "OrderMs");
            }
            Members member = membersService.GetByID(Session["Memberid"]);
            Memberblacklist blacklist = new Memberblacklist();            
            string ipaddress;
            ipaddress = Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            if(ipaddress == "" || ipaddress == null)
            {
                ipaddress = Request.ServerVariables["REMOTE_ADDR"];
            }
            if (order.Url.IndexOf("facebook.com") != -1 && order.Count != null)
            {
                if(order.Url.IndexOf("photos") != -1 || order.Url.IndexOf("posts") != -1 || order.Url.IndexOf("video") != -1 || order.Url.IndexOf("permalink") != -1 || order.Url.IndexOf("photo") != -1)
                {
                    string Url = order.Url.Replace(" ", "");         // 將訂單的空白字元砍掉
                    Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
                    int? MemberCooldown = member.Memberlevel.Memberlevelcooldown.FirstOrDefault().Cooldowntime;           // 該會員的冷卻時間(一般/VIP)

                    if (member.Isreal == true)
                    {
                        Guid Realid = memberlevelService.Get().Where(a => a.Levelname == "真人").FirstOrDefault().Levelid;        // 取得真人ID
                        int? RealCooldowntime = memberlevelcooldownService.Get().Where(a => a.Levelid == Realid).FirstOrDefault().Cooldowntime;      // 取得真人的冷卻時間
                        if (MemberCooldown > RealCooldowntime)
                        {
                            Cooldowntime = RealCooldowntime;
                        }
                        else
                        {
                            Cooldowntime = MemberCooldown;
                        }
                    }
                    else
                    {
                        Cooldowntime = MemberCooldown;
                    }
                    IEnumerable<Order> old_order = orderService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(o => o.Createdate);
                    if (old_order.ToList().Count() == 0)
                    {
                        if (TryUpdateModel(order, new string[] { "Count" }) && ModelState.IsValid)
                        {
                            order.Orderid = Guid.NewGuid();
                            order.Createdate = DateTime.Now;
                            order.Updatedate = DateTime.Now;
                            order.Memberid = Memberid;
                            order.Remains = order.Count;
                            order.Url = Url;
                            order.Ordernumber = "heoorder" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                            Session["OrderNumber"] = order.Ordernumber;
                            order.Service = "讚";
                            orderService.Create(order);
                            orderService.SaveChanges();
                        }
                        return RedirectToAction("OrderResult");
                    }
                    else
                    {
                        DateTime date = old_order.FirstOrDefault().Createdate.AddSeconds(Convert.ToDouble(Cooldowntime));

                        if (DateTime.Now > date)
                        {
                            if (TryUpdateModel(order, new string[] { "Count", }) && ModelState.IsValid)
                            {
                                order.Orderid = Guid.NewGuid();
                                order.Createdate = DateTime.Now;
                                order.Updatedate = DateTime.Now;
                                order.Memberid = Memberid;
                                order.Remains = order.Count;
                                order.Url = Url;
                                order.Ordernumber = "heoorder" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                                Session["OrderNumber"] = order.Ordernumber;
                                order.Service = "讚";
                                orderService.Create(order);
                                orderService.SaveChanges();
                            }
                            return RedirectToAction("OrderResult");
                        }
                        else
                        {
                            double time = ((date - DateTime.Now).TotalSeconds);
                            Session["Date"] = Convert.ToInt16(time);
                            return RedirectToAction("OrderCooldown");
                        }
                    }
                }
                else
                {
                    ViewBag.TotalNumber = membersService.Get().Count();         // 會員總人數
                    Setting Setting = settingService.Get().FirstOrDefault();
                    ViewBag.Max = Setting.Max;
                    ViewBag.Min = Setting.Min;
                    TempData["message"] = "網址輸入錯誤，請重新下單!!!";
                    return RedirectToAction("Order", "OrderMs");
                }
            }
            else if(order.Url.Contains("'") || order.Url.Contains("\"") || order.Count == null)     // 亂輸入者，則被寫到黑名單的表裡面，並記載IP、Useragent、MemberId
            {                
                blacklist.Account = member.Account;
                blacklist.Memberid = Guid.Parse(Session["Memberid"].ToString());
                blacklist.Useragent = Request.UserAgent;
                blacklist.IP_Addr = ipaddress;
                memberblacklistService.Create(blacklist);
                memberblacklistService.SaveChanges();
                Session.RemoveAll();
                return RedirectToAction("Home", "HomeMs");
            }
            blacklist.Account = member.Account;
            blacklist.Memberid = Guid.Parse(Session["Memberid"].ToString());
            blacklist.Useragent = Request.UserAgent;
            blacklist.IP_Addr = ipaddress;
            memberblacklistService.Create(blacklist);
            memberblacklistService.SaveChanges();
            Session.RemoveAll();
            return RedirectToAction("Home", "HomeMs");
        }

        [CheckSession]
        public ActionResult OrderResult()
        {
            Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
            IEnumerable<Order> order = orderService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(o => o.Createdate);
            ViewBag.OrderNumber = Session["OrderNumber"];
            ViewBag.Url = order.FirstOrDefault().Url;
            ViewBag.Count = order.FirstOrDefault().Count;         
            return View();
        }

        [CheckSession] 
        public ActionResult OrderCooldown()
        {
            ViewBag.Date = Session["Date"];
            return View();
        }
    }
    #region --目前登入人數--
    public class OrderLoginHub : Hub
    {
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private SettingService settingService;
        public OrderLoginHub()
        {
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            settingService = new SettingService();        
        }

        public void userConnected()
        {
            Setting setting = settingService.Get().FirstOrDefault();                
            //Guid Vipid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800;      // 目前時間的總秒數
            var query = membersService.Get().Where(x => x.Is_import == 0 || x.Is_import == 2).Where(a => a.Logindate >= Now).Count();
            Clients.User(Context.ConnectionId);
            Clients.All.getList(query);                  
        }

        //public override Task OnDisconnected(bool Order = true)
        //{
        //    Clients.All.removeList(Context.ConnectionId);
        //    var item = Userdata.FirstOrDefault(a => a.Id == Context.ConnectionId);
        //    if (item != null)
        //    {
        //        Userdata.Remove(item);
        //        Clients.All.onUserDisconnected(item.Id);
        //    }
        //    return base.OnDisconnected(true);
        //}
    }
    #endregion  
    #region --訂單成功--
    public class OrderSuccessHub : Hub
    {
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private SettingService settingService;
        private OrderService orderService;
        public OrderSuccessHub()
        {
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            settingService = new SettingService();
            orderService = new OrderService();
        }

        public void userConnected(string OrderNumber)
        {
            var query = orderService.Get().Where(a => a.Ordernumber == OrderNumber).FirstOrDefault().OrderStatus;
            Clients.Client(connectionId: Context.ConnectionId).getList(query);
        }
    }
    #endregion

    public class OrderData
    {
        public string Id { get; set; }
        public string Url { get; set; }
    }

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // Any connection or hub wire up and configuration should go here
            app.MapSignalR();
        }
    }
}