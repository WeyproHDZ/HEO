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
    public class OrderMsController : BaseController
    {
        private OrderService orderService;
        private MembersService membersService;
        private SettingService settingService;
        public OrderMsController()
        {
            orderService = new OrderService();
            membersService = new MembersService();
            settingService = new SettingService();
        }
        // GET: OrderMs
        [CheckSession]
        public ActionResult Order()
        {
            Setting Setting = settingService.Get().FirstOrDefault();
            ViewBag.Max = Setting.Max;
            ViewBag.Min = Setting.Min;
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Order(Order order)
        {
            Members member = membersService.GetByID(Session["Memberid"]);
            Guid Memberid = Guid.Parse(Session["Memberid"].ToString());
            Memberlevelcooldown memberlevelcooldown = member.Memberlevel.Memberlevelcooldown.FirstOrDefault();
            IEnumerable<Order> old_order = orderService.Get().Where(a => a.Memberid == Memberid).OrderByDescending(o => o.Createdate);
            if(old_order.ToList().Count() == 0)
            {
                if (TryUpdateModel(order, new string[] { "Count", "Url" }) && ModelState.IsValid)
                {
                    order.Orderid = Guid.NewGuid();
                    order.Createdate = DateTime.Now;
                    order.Updatedate = DateTime.Now;
                    order.Memberid = Memberid;
                    order.Ordernumber = "heoorder" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                    order.Service = "讚";
                    orderService.Create(order);
                    orderService.SaveChanges();
                }
                return RedirectToAction("OrderResult");
            }
            else
            {
                DateTime date = old_order.FirstOrDefault().Createdate.AddSeconds(Convert.ToDouble(memberlevelcooldown.Cooldowntime));

                if (DateTime.Now > date)
                {
                    if (TryUpdateModel(order, new string[] { "Count", "Url" }) && ModelState.IsValid)
                    {
                        order.Orderid = Guid.NewGuid();
                        order.Createdate = DateTime.Now;
                        order.Updatedate = DateTime.Now;
                        order.Memberid = Memberid;
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