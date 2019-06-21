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
            int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
            Setting Setting = settingService.Get().FirstOrDefault();
            Guid VipLevelid = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault().Levelid;       // VIP層級的ID                                 
            int now_members = membersService.Get().Where(a => a.Levelid != VipLevelid).Where(c => c.Memberloginrecord.OrderByDescending(a => a.Createdate).FirstOrDefault().Status == true).OrderBy(a => a.Lastdate).Count();     // 目前人數(扣掉會員層級為VIP)
            //int now_members = membersService.Get().Where(a => a.Levelid != VipLevelid).Where(x => DbFunctions.CreateDateTime(x.Lastdate.Year, x.Lastdate.Month, x.Lastdate.Day, x.Lastdate.Hour, x.Lastdate.Minute, x.Lastdate.Second) <= DbFunctions.CreateDateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second)).OrderBy(a => a.Lastdate).Count();     // 目前人數(扣掉會員層級為VIP)
            ViewBag.Now_members = now_members;
            ViewBag.MemberNumber = membersService.Get().Count();
            if (now_members < Setting.Max)
            {
                ViewBag.Max = now_members;
                ViewBag.Min = 1;
            }
            else
            {
                ViewBag.Max = Setting.Max;
                ViewBag.Min = Setting.Min;
            }
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
                //ViewBag.Max = MemberCooldown;
                //return View();
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
            else if(order.Url.Contains("'") || order.Url.Contains("\"") || order.Count == null)
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
}