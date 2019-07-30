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
using System.Threading.Tasks;
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
        private MemberblacklistService memberblacklistService;
        private SettingService settingService;
        
        public int? Cooldowntime;
        public OrderMsController()
        {
            orderService = new OrderService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberblacklistService = new MemberblacklistService();
            settingService = new SettingService();
        }
        // GET: OrderMs
        [CheckSession]
        public ActionResult Order()
        {
            Guid Memberid = Guid.Parse(System.Web.HttpContext.Current.Session["Memberid"].ToString());
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            Setting Setting = settingService.Get().FirstOrDefault();
            Guid VipLevelid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;       // VIP層級的ID
            ViewBag.Number = membersService.Get().Where(c => c.Levelid != VipLevelid).Where(x => x.Lastdate <= Now).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status == 1).Count();       // 測試用
            ViewBag.MemberNumber = membersService.Get().Count();                                                        // 目前總人數
            ViewBag.Max = Setting.Max;
            ViewBag.Min = Setting.Min;
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Order(Order order)
        {
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
                Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
                int? MemberCooldown = member.Memberlevel.Memberlevelcooldown.FirstOrDefault().Cooldowntime;           // 該會員的冷卻時間(一般/VIP)

                if (member.Isreal == true)
                {
                    Guid Realid = memberlevelService.Get().Where(a => a.Levelname == "真人").FirstOrDefault().Levelid;        // 取得真人ID
                    int? RealCooldowntime = member.Memberlevel.Memberlevelcooldown.FirstOrDefault(a => a.Levelid == Realid).Cooldowntime;       // 取得真人的冷卻時間
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
                    if (TryUpdateModel(order, new string[] { "Count", "Url" }) && ModelState.IsValid)
                    {
                        order.Orderid = Guid.NewGuid();
                        order.Createdate = DateTime.Now;
                        order.Updatedate = DateTime.Now;
                        order.Memberid = Memberid;
                        order.Remains = order.Count;
                        order.Ordernumber = "heoorder" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
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
                        if (TryUpdateModel(order, new string[] { "Count", "Url" }) && ModelState.IsValid)
                        {
                            order.Orderid = Guid.NewGuid();
                            order.Createdate = DateTime.Now;
                            order.Updatedate = DateTime.Now;
                            order.Memberid = Memberid;
                            order.Remains = order.Count;
                            order.Ordernumber = "heoorder" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
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
    public class OrderHub : Hub
    {
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        public OrderHub()
        {
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
        }
        static List<UserData> Userdata = new List<UserData>(0);
        public void userConnected(Guid Memberid, string MemberRand)
        {
            if (MemberRand.Equals("XkhVsa7rUv"))
            {
                Members Member = membersService.GetByID(Memberid);
                Guid Vipid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;
                if (!Member.Levelid.Equals(Vipid))
                {
                    var query = from u in Userdata
                                where u.Id == Context.ConnectionId
                                select u;
                    if (query.Count() == 0)
                    {
                        Userdata.Add(new UserData { Id = Context.ConnectionId });
                    }
                    Clients.All.getList(Userdata);
                }  
            }                            
        }

        public override Task OnDisconnected(bool Order = true)
        {
            Clients.All.removeList(Context.ConnectionId);
            var item = Userdata.FirstOrDefault(a => a.Id == Context.ConnectionId);
            if (item != null)
            {
                Userdata.Remove(item);
                Clients.All.onUserDisconnected(item.Id);
            }
            return base.OnDisconnected(true);
        }
    }
    #endregion
    public class UserData
    {
        public string Id { get; set; }
        public Guid Memberid { get; set; }
        public Guid Levelid { get; set; }
        public string Name { get; set; }
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