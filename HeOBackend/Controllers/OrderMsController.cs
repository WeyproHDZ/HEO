using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using HeOBackend;
using HeO.Models;
using HeO.Service;

namespace HeOBackend.Controllers
{
    public class OrderMsController : BaseController
    {
        private HeOEntities db;
        private OrderService orderService;
        private OrderfacebooklistService orderfacebooklistService;
        private FeedbackproductService feedbackproductService;
        private MemberloginrecordService memberloginrecordService;

        public OrderMsController()
        {
            db = new HeOEntities();
            orderService = new OrderService();
            orderfacebooklistService = new OrderfacebooklistService();
            feedbackproductService = new FeedbackproductService();
            memberloginrecordService = new MemberloginrecordService();
        }
        // GET: OrderMs
        [CheckSession(IsAuth = true)]
        public ActionResult Order(int p = 1)
        {
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            var data = orderService.Get().OrderByDescending(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.nextpageNumber = 1;
            ViewBag.Order = data.ToPagedList(pageNumber: p, pageSize: 20);
            ViewBag.feedbackproduct = feedbackproduct;
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult Order(string search, int p = 1)
        {
            if(search != null)
            {
                IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
                var data = orderService.Get().Where(x => x.Ordernumber.Contains(search)).OrderBy(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.nextpageNumber = 1;
                ViewBag.Order = data.ToPagedList(pageNumber: p, pageSize: 20);
                ViewBag.feedbackproduct = feedbackproduct;
                return View();
            }
            else
            {
                IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
                var data = orderService.Get().OrderBy(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.nextpageNumber = 1;
                ViewBag.Order = data.ToPagedList(pageNumber: p, pageSize: 20);
                ViewBag.feedbackproduct = feedbackproduct;
                return View();
            }
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult ViewOrderfacebooklist(Guid Orderid , int p , int np)
        {
            IEnumerable<Orderfaceooklist> orderfacebooklist = orderfacebooklistService.Get().Where(a => a.Orderid == Orderid).OrderBy(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.nextpageNumber = np;
            ViewBag.Orderid = Orderid;
            ViewBag.orderfacebooklist = orderfacebooklist.ToPagedList(pageNumber: np, pageSize: 20);
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditOrder(Guid Orderid, int p)
        {
            ViewBag.pageNumber = p;
            Order order = orderService.GetByID(Orderid);
            return View(order);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditOrder(Guid Orderid, string Orderstatus)
        {
            Order order = orderService.GetByID(Orderid);
            order.OrderStatus = Convert.ToInt32(Orderstatus);
            orderService.Update(order);
            orderService.SaveChanges();
            return RedirectToAction("Order");
        }
    }
}