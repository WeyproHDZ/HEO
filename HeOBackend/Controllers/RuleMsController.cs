using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HtmlAgilityPack;
using PagedList;
using System.IO;
using HeOBackend;
using HeO.Models;
using HeO.Service;

namespace HeOBackend.Controllers
{
    public class RuleMsController : BaseController
    {
        private HeOEntities db;
        private TermService termService;
        private TermlangService termlangService;
        private GuideService guideService;
        private GuidelangService guidelangService;
        public RuleMsController()
        {
            db = new HeOEntities();
            termService = new TermService();
            termlangService = new TermlangService();
            guideService = new GuideService();
            guidelangService = new GuidelangService();
        }
        // GET: RuleMs
        /**** 使用條款 新增/刪除/修改/更新排序 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Term(int p = 1)
        {
            var data = termlangService.Get().OrderBy(o => o.Term.Orders);
            ViewBag.pageNumber = p;
            ViewBag.Term = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddTerm()
        {
            return View();
        }
        [CheckSession(IsAuth = true)]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddTerm(Term term , Termlang lang)
        {
            term.Termid = Guid.NewGuid();
            term.Uuid = Guid.NewGuid();
            term.Createdate = DateTime.Now;
            term.Updatedate = DateTime.Now;
            termService.Create(term);
            termService.SaveChanges();

            if(TryUpdateModel(lang , new string[] { "title" , "contents" }) && ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return RedirectToAction("Term");
            }
            else
            {
                lang.Langid = 1;
                lang.Termid = term.Termid;
                termlangService.Create(lang);
                termlangService.SaveChanges();
                return RedirectToAction("Term");
            }
        }

        [CheckSession (IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteTerm(Guid termid , int p)
        {
            Term term = termService.GetByID(termid);

            termService.Delete(term);
            termService.SaveChanges();

            return RedirectToAction("Term", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditTerm(Guid termid , int p)
        {
            ViewBag.pageNumber = p;
            Term term = termService.GetByID(termid);
            return View(term);
        }
        [CheckSession(IsAuth = true)]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditTerm(Guid termid , Termlang lang , Guid Uuid)
        {
            Term term = termService.GetByID(termid);

            if(TryUpdateModel(lang , new string[] { "title" , "contents" }) && ModelState.IsValid)
            {
                term.Uuid = Guid.NewGuid();
                termService.Update(term);
                termService.SaveChanges();
                termlangService.Update(lang);
                termlangService.SaveChanges();

                return RedirectToAction("Term");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(lang);
            }
        }

        [CheckSession(IsAuth = true)]
        public ActionResult SortTerm(int p, IEnumerable<Term> EntityLists = null)
        {
            foreach (Term term in EntityLists)
            {
                termService.SpecificUpdate(term, new string[] { "Orders" });
            }
            termService.SaveChanges();
            return RedirectToAction("Term", new { p = p });
        }
        /**** 使用導引 新增/刪除/修改/更新排序 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Guide(int p = 1)
        {
            var data = guidelangService.Get().OrderBy(o => o.Guide.Orders);
            ViewBag.pageNumber = p;
            ViewBag.Guide = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddGuide()
        {
            return View();
        }
        [CheckSession(IsAuth = true)]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult AddGuide(Guide guide, Guidelang lang)
        {
            guide.Guideid = Guid.NewGuid();
            guide.Uuid = Guid.NewGuid();
            guide.Createdate = DateTime.Now;
            guide.Updatedate = DateTime.Now;
            guideService.Create(guide);
            guideService.SaveChanges();
            if (TryUpdateModel(lang, new string[] { "title", "contents" }) && ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(lang);
            }
            else
            {
                lang.Guideid = guide.Guideid;
                lang.Langid = 1;
                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(lang.Contents);
                guidelangService.Create(lang);
                guidelangService.SaveChanges();
                return RedirectToAction("Guide");
            }
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteGuide(Guid guideid , int p)
        {
            Guide guide = guideService.GetByID(guideid);

            guideService.Delete(guide);
            guideService.SaveChanges();
            return RedirectToAction("Guide" , new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditGuide(Guid guideid , int p)
        {
            ViewBag.pageNumber = p;
            Guide guide = guideService.GetByID(guideid);
            return View(guide);
        }
        [CheckSession(IsAuth = true)]
        [ValidateInput(false)]
        [HttpPost]
        public ActionResult EditGuide(Guid guideid , Guidelang lang , Guid Uuid)
        {
            Guide guide = guideService.GetByID(guideid);
            guide.Uuid = Guid.NewGuid();
            guideService.Update(guide);
            guideService.SaveChanges();
            if (TryUpdateModel(lang, new string[] { "Title", "Contents" }) && ModelState.IsValid)
            {
                guidelangService.Update(lang);
                guidelangService.SaveChanges();

                return RedirectToAction("Guide");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(lang);
            }
        }

        [CheckSession(IsAuth = true)]
        public ActionResult SortGuide(int p, IEnumerable<Guide> EntityLists = null)
        {
            foreach (Guide guide in EntityLists)
            {
                guideService.SpecificUpdate(guide, new string[] { "Orders" });
            }
            guideService.SaveChanges();
            return RedirectToAction("Guide", new { p = p });
        }
    }
}