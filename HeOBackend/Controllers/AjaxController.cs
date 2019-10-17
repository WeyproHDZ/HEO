using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeO.Models;
using HeO.Service;
namespace HeOBackend.Controllers
{
    public class AjaxController : BaseController
    {
        private MembersService membersService;
        private MemberblacklistService memberblacklistService;

        public AjaxController()
        {
            membersService = new MembersService();
            memberblacklistService = new MemberblacklistService();
        }
        // GET: Ajax
        /***** 批量刪除 ****/
        public JsonResult AjaxDeleteChecked(Guid[] Memberid)
        {
            if(Memberid != null)
            {
                foreach (Guid thismemberid in Memberid)
                {
                    Members Members = membersService.GetByID(thismemberid);
                    Members.Isenable = 0;
                    membersService.SpecificUpdate(Members, new string[] { "Isenable" });
                }
                membersService.SaveChanges();
            }
            return this.Json("Success");
        }

        /**** 加入黑名單 ****/
        public JsonResult AjaxBlackChecked(Guid[] Memberid)
        {
            if(Memberid != null)
            {
                foreach (Guid thismemberid in Memberid)
                {
                    Memberblacklist Memberblacklist = new Memberblacklist();
                    Members Members = membersService.GetByID(thismemberid);
                    Memberblacklist.Memberid = Members.Memberid;
                    Memberblacklist.Account = Members.Account;
                    Memberblacklist.Useragent = Members.Useragent_phone;
                    Memberblacklist.Createdate = DateTime.Now;
                    memberblacklistService.Create(Memberblacklist);                                        
                }
                memberblacklistService.SaveChanges();
            }
            return this.Json("Success");
        }

        /**** 轉入前台 ****/
        public JsonResult AjaxToFrontend(Guid[] Memberid)
        {
            if(Memberid != null)
            {
                foreach(Guid thismemberid in Memberid)
                {
                    Members member = membersService.GetByID(thismemberid);
                    member.Is_import = 2;       // 是否匯入 【0 : 前台登入 , 1 : 後台匯入 , 2 : 轉前台】           
                }
                membersService.SaveChanges();
            }
            return this.Json("Success");
        }
    }
}