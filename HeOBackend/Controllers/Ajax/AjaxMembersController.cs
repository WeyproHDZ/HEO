using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeO.Service;
using HeO.Models;
namespace HeOBackend.Controllers
{
    public class AjaxMembersController : BaseController
    {
        private MembersService membersService;
        private MemberblacklistService memberblacklistService;

        public AjaxMembersController()
        {
            membersService = new MembersService();
            memberblacklistService = new MemberblacklistService();
        }
        // GET: AjaxMembers
        public JsonResult AjaxMembersData(int start, int length)
        {
            string[] data = new string[12];
            string search = Request.QueryString["search[value]"];
            ViewBag.search = search;

            //var data = membersService.Get().OrderByDescending(o => o.Createdate);
            var members = membersService.Get();
            members = members.Where(a => a.Account == search || a.Name == search).Take(length);
            members = members.OrderByDescending(o => o.Createdate).ToList();
            //if (members != null)
            //{
            //    foreach (Members list in members)
            //    {

            //        switch (list.Is_import)
            //        {
            //            case 0:
            //                data[0] = "<td class='text-center'><i class='fa fa-male' aria-hidden='true' style='color:green'></i></td>";
            //                break;
            //            case 1:
            //                data[0] = "<td class='text - center'><i class='fa fa-android' aria-hidden='true' style='color: red'></i></td>";
            //                break;
            //            default:
            //                data[0] = "<td class='text - center'><i class='fa fa-android' aria-hidden='true' style='color: blue'></i></td>";
            //                break;
            //        }
            //    }
            //}
            //ViewBag.message = "按下確定後開始驗證帳號，請勿關閉分頁";

            ///*** 驗證狀態人數統計 ***/
            //ViewBag.Question = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 0).Count();
            //ViewBag.Check = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1).Count();
            //ViewBag.Times = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 2).Count();

            ///**** 機器人、前台登入、仿前台登入機器人的人數 ****/
            //ViewBag.Robot = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1).Where(a => a.Is_import == 1).Count();
            //ViewBag.FrontLoginNumber = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 1).Where(a => a.Is_import == 0).Count();
            //ViewBag.RobotFrontLoginNumber = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(o => o.Createdate).FirstOrDefault().Status == 1).Where(a => a.Is_import == 2).Count();
            return this.Json(members.ToArray() , JsonRequestBehavior.AllowGet);
        }
    }
}