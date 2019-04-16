﻿using System;
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

        public OrderMsController()
        {
            db = new HeOEntities();
            orderService = new OrderService();
            orderfacebooklistService = new OrderfacebooklistService();
            feedbackproductService = new FeedbackproductService();
        }
        // GET: OrderMs
        [CheckSession(IsAuth = true)]
        public ActionResult Order(int p = 1)
        {
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            var data = orderService.Get().OrderBy(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.Order = data.ToPagedList(pageNumber: p, pageSize: 20);
            ViewBag.feedbackproduct = feedbackproduct;
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult ViewOrderfacebooklist(Guid Orderid , int p = 1)
        {
            IEnumerable<Orderfaceooklist> orderfacebooklist = orderfacebooklistService.Get().Where(a => a.Orderid == Orderid).OrderBy(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.orderfacebooklist = orderfacebooklist.ToPagedList(pageNumber: p, pageSize: 20);
            return View();
        }
    }
}