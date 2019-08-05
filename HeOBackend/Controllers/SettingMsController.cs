using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using PagedList;
using HeOBackend;
using HeO.Models;
using HeO.Service;
using HtmlAgilityPack;

namespace HeOBackend.Controllers
{
    public class SettingMsController : BaseController
    {
        private HeOEntities db;
        private AdminsService adminsService;
        private LimsSerivce limsService;
        private AdminLimsService adminlimsService;
        
        public SettingMsController()
        {
            db = new HeOEntities();
            adminsService = new AdminsService();
            limsService = new LimsSerivce();
            adminlimsService = new AdminLimsService();
        }
        [CheckSession(IsAuth = true)]
        public ActionResult Admins(int p = 1)
        {
            var data = adminsService.Get().Where(a => a.Isenable == 1).OrderBy(o => o.AdminID);

            ViewBag.pageNumber = p;
            ViewBag.Admins = data.ToPagedList(pageNumber: p, pageSize: 20);
            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddAdmins()
        {
            ViewBag.Lims = limsService.Get().Where(a => a.ParentID == null).OrderBy(a => a.Sort);
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddAdmins(Admins admin)
        {
            foreach (var ms in ModelState.ToArray())
            {
                if (ms.Key.StartsWith("AdminLims["))
                {
                    ModelState.Remove(ms);
                }
            }

            if (TryUpdateModel(admin, new string[] { "username", "password" }) && ModelState.IsValid)
            {
                admin.Isenable = 1;

                if (admin.AdminLims != null)
                {
                    foreach (AdminLims adminlim in admin.AdminLims.ToArray())
                    {
                        if (adminlim.LimID != 0)
                        {
                            adminlim.AdminLimID = Guid.NewGuid();
                        }
                        else
                        {
                            admin.AdminLims.Remove(adminlim);
                        }
                    }
                }

                adminsService.Create(admin);
                adminsService.SaveChanges();

                return RedirectToAction("Admins");
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ViewBag.Lims = limsService.Get().Where(a => a.ParentID == null).OrderBy(a => a.Sort);
                return View(admin);
            }
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditAdmins(int adminid, int p)
        {
            ViewBag.pageNumber = p;
            ViewBag.Lims = limsService.Get().Where(a => a.ParentID == null).OrderBy(a => a.Sort);
            Admins admin = adminsService.GetByID(adminid);

            return View(admin);
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditAdmins(int adminid, int p, ICollection<AdminLims> AdminLims, int isenable = 0, string password = null)
        {
            Admins admin = adminsService.GetByID(adminid);

            if (ModelState.IsValid)
            {
                admin.Isenable = Convert.ToByte(isenable);
                if (password != null && password != "") admin.Password = password;

                if (AdminLims != null)
                {
                    AdminLims = AdminLims.Where(a => a.LimID != 0).ToList();

                    foreach (AdminLims adminlim in admin.AdminLims.ToArray())
                    {
                        if (!AdminLims.ToList().Exists(a => a.LimID == adminlim.LimID))
                        {
                            admin.AdminLims.Remove(adminlim);
                            adminlimsService.Delete(adminlim.AdminLimID);
                        }
                    }

                    foreach (AdminLims al in AdminLims)
                    {
                        if (admin.AdminLims.ToList().Exists(a => a.LimID == al.LimID))
                        {
                            AdminLims cd = admin.AdminLims.Where(a => a.LimID == al.LimID).FirstOrDefault();
                            cd.IsAdd = al.IsAdd;
                            cd.IsUpdate = al.IsUpdate;
                            cd.IsDelete = al.IsDelete;
                        }
                        else
                        {
                            al.AdminLimID = Guid.NewGuid();
                            al.AdminID = admin.AdminID;
                            admin.AdminLims.Add(al);
                        }
                    }
                }

                adminsService.Update(admin);
                adminsService.SaveChanges();

                return RedirectToAction("Admins", new { p = p });
            }
            else
            {
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                ViewBag.pageNumber = p;
                ViewBag.Lims = limsService.Get().Where(a => a.ParentID == null).OrderBy(a => a.Sort);
                return View(admin);
            }
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteAdmins(int adminid, int p)
        {
            Admins admin = adminsService.GetByID(adminid);

            admin.Isenable = 0;

            adminsService.SaveChanges();
            return RedirectToAction("Admins", new { p = p });
        }
    }
}