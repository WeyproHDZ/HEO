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
    public class RuleMsController : BaseController
    {
        private TermService termService;
        private TermlangService termlangService;
        private GuideService guideService;
        private GuidelangService guidelangService;

        public RuleMsController()
        {
            termService = new TermService();
            termlangService = new TermlangService();
            guideService = new GuideService();
            guidelangService = new GuidelangService();
        }
        // GET: RuleMs
        [HttpGet]
        public ActionResult Guide(Guid? Guideid)
        {
            ViewBag.Guide = guideService.Get().OrderBy(o => o.Orders);
            if(Guideid != null)
            {
                Guide GuideContents = guideService.GetByID(Guideid);
                //ViewBag.GuideContents = GuideContents.Guidelang.FirstOrDefault(a => a.Langid == 1);
                ViewBag.Title = GuideContents.Guidelang.FirstOrDefault(a => a.Langid == 1).Title;
                ViewBag.Contents = GuideContents.Guidelang.FirstOrDefault(a => a.Langid == 1).Contents;
            }
            else
            {
                IEnumerable<Guide> GuideContents = guideService.Get().OrderBy(o => o.Orders);
                ViewBag.Title = GuideContents.FirstOrDefault().Guidelang.FirstOrDefault(a => a.Langid == 1).Title;
                ViewBag.Contents = GuideContents.FirstOrDefault().Guidelang.FirstOrDefault(a => a.Langid == 1).Contents;
            }
            return View();
        }

        [HttpGet]
        public ActionResult Term()
        {
            ViewBag.Term = termService.Get().OrderBy(o => o.Orders);
            return View();
        }
    }
}