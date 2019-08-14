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
    public class HomeMsController : BaseController
    {
        private HeOEntities db;
        private NewsService newsService;
        private NewslangService newslangService;
        private SettingService settingService;

        public HomeMsController()
        {
            db = new HeOEntities();
            newsService = new NewsService();
            newslangService = new NewslangService();
            settingService = new SettingService();
        }
        // GET: HomeMs
        [CheckSession(IsAuth = true)]
        public ActionResult News(int p = 1)
        {
            var data = newslangService.Get().OrderByDescending(o => o.News.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.News = data.ToPagedList(pageNumber: p, pageSize: 20);
            
            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddNews()
        {
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddNews(News news , Newslang lang , DateTime date)
        {
            news.Newsid = Guid.NewGuid();
            news.Uuid = Guid.NewGuid();
            news.Createdate = DateTime.Now;
            news.Updatedate = DateTime.Now;
            news.Date = date.ToString("yyyy/MM/dd");
            newsService.Create(news);
            newsService.SaveChanges();

            if (TryUpdateModel(lang, new string[] { "title"}) && ModelState.IsValid)
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return RedirectToAction("News");
            }
            else
            {
                lang.Langid = 1;
                lang.Newsid = news.Newsid;
                newslangService.Create(lang);
                newslangService.SaveChanges();

                return RedirectToAction("News");
            }
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditNews(Guid newsid , int p)
        {
            ViewBag.pageNumber = p;
            News news = newsService.GetByID(newsid);
            return View(news);
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditNews(Guid newsid , Newslang lang,Guid Uuid,DateTime Date)
        {
            News news = newsService.GetByID(newsid);

            if (TryUpdateModel(lang, new string[] { "title" }) && ModelState.IsValid)
            {
                news.Uuid = Guid.NewGuid();
                news.Date = Date.ToShortDateString();
                newsService.Update(news);
                newsService.SaveChanges();
                newslangService.Update(lang);
                newslangService.SaveChanges();

                return RedirectToAction("News");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(lang);
            }
        }
        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteNews(Guid newsid , int p)
        {
            News news = newsService.GetByID(newsid);

            newsService.Delete(news);
            newsService.SaveChanges();

            return RedirectToAction("News", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditReciprocity(Guid settingid , int p)
        {
            ViewBag.settingid = settingid;
            ViewBag.pageNumber = p;
            Setting reciprocity = settingService.GetByID(settingid);
            return View(reciprocity);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditReciprocity(Guid settingid)
        {
            Setting reciprocity = settingService.GetByID(settingid);
            if (TryUpdateModel(reciprocity, new string[] { "Max" , "Min" , "Time" }) && ModelState.IsValid)
            {
                reciprocity.Updatedate = DateTime.Now;
                settingService.Update(reciprocity);
                settingService.SaveChanges();

                return RedirectToAction("EditReciprocity",new { settingid = settingid  , p = 1});
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                return View(reciprocity);
            }
        }
    }
}