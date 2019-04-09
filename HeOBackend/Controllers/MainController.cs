using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeOBackend;
using HeO.Models;


namespace HeOBackend.Controllers
{
    public class MainController : BaseController
    {
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
            else
            {
                return false;
            }
        }
    }
}