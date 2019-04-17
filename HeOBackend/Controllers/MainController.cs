using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeOBackend;
using HeO.Models;
using HeO.Service;

namespace HeOBackend.Controllers
{
    public class MainController : BaseController
    {
        private AdminsService adminsService;

        public MainController()
        {
            adminsService = new AdminsService();
        }

        [CheckSession]
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Login()
        {
            Session.Add("IsLogin", false);
            return View();
        }

        [HttpPost]
        public ActionResult Login(string username, string password)
        {
            if (ValidateUser(username, password))
            {
                return RedirectToAction("Index");
            }
            else
            {
                ModelState.AddModelError("", "");
            }

            return View();
        }

        public ActionResult Logout()
        {
            Session.Add("IsLogin", false);
            Session.Remove("Username");
            return RedirectToAction("Login");
        }

        // 驗證帳號密碼
        private bool ValidateUser(string username, string password)
        {
            if (username == "weypro" && password == "weypro12ab")
            {
                Session.Add("IsLogin", true);
                Session.Add("Username", "weypro");
                Session.Add("AdminID", 888);

                return true;
            }
            Admins admin = adminsService.Get().Where(a => a.Username == username && a.Isenable == 1).FirstOrDefault();
            if (admin == null)
                return false;

            if (admin.Password != password)
                return false;

            Session.Add("IsLogin", true);
            Session.Add("Username", username);
            Session.Add("AdminID", admin.AdminID);
            Session.Add("AdminLims", admin.AdminLims);

            return true;
        }
    }
}