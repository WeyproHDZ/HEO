using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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
    public class UseragentMsController : BaseController
    {
        private HeOEntities db;
        private UseragentService useragentService;
        public UseragentMsController()
        {
            db = new HeOEntities();
            useragentService = new UseragentService();
        }
        // GET: Useragent
        [CheckSession(IsAuth = true)]
        public ActionResult Useragent(int p = 1)
        {
            var data = useragentService.Get().OrderBy(a => a.Id);
            ViewBag.pageNumber = p;
            ViewBag.Useragent = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddUseragent()
        {
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddUseragent(Useragent useragent)
        {            
            useragentService.Create(useragent);
            useragentService.SaveChanges();
            return RedirectToAction("Useragent");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditUseragent(int Id, int p)
        {
            ViewBag.pageNumber = p;
            Useragent useragent = useragentService.GetByID(Id);
            return View(useragent);
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditUseragent(int Id, Useragent useragent)
        {
            if (TryUpdateModel(useragent, new string[] { "Useragent", "Isweb" }) && ModelState.IsValid)
            {
                useragentService.Update(useragent);
                useragentService.SaveChanges();

                return RedirectToAction("Useragent");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(useragent);
            }
        }
    }
}