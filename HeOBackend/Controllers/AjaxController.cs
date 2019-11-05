using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeO.Models;
using HeO.Service;
using System.Net;
using System.Text;
using System.IO;

namespace HeOBackend.Controllers
{
    public class AjaxController : BaseController
    {
        private MembersService membersService;
        private MemberblacklistService memberblacklistService;
        private MemberloginrecordService memberloginrecordService;

        public AjaxController()
        {
            membersService = new MembersService();
            memberblacklistService = new MemberblacklistService();
            memberloginrecordService = new MemberloginrecordService();
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

        /***** 驗證帳號 ****/
        public JsonResult AjaxAuthMembers(Guid[] Memberid)
        {
            if (Memberid != null)
            {
                foreach (Guid thismemberid in Memberid)
                {
                    Members Members = membersService.GetByID(thismemberid);
                    string url = "http://cp4m.heohelp.com:8080/Check/BackendCkeckFacebook?Facebookid=" + Members.Facebookid;
                    WebRequest myReq = WebRequest.Create(url);
                    myReq.Method = "GET";
                    myReq.ContentType = "application/json; charset=UTF-8";
                    UTF8Encoding enc = new UTF8Encoding();
                    myReq.Headers.Remove("auth-token");
                    WebResponse wr = myReq.GetResponse();
                    Stream receiveStream = wr.GetResponseStream();
                    StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
                    string content = reader.ReadToEnd();
                    content = content.Replace("\"", "");
                    if (content != "")       // 假設content不是空值時
                    {
                        if (content == "已驗證")
                        {
                            Memberloginrecord loginrecord = new Memberloginrecord();
                            loginrecord.Status = 1;
                            loginrecord.Memberid = Members.Memberid;
                            loginrecord.Createdate = DateTime.Now;
                            memberloginrecordService.Create(loginrecord);
                        }
                        else
                        {
                            Memberloginrecord loginrecord = new Memberloginrecord();
                            loginrecord.Status = 2;
                            loginrecord.Memberid = Members.Memberid;
                            loginrecord.Createdate = DateTime.Now;
                            memberloginrecordService.Create(loginrecord);
                        }
                    }
                    else
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 0;
                        loginrecord.Memberid = Members.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                }
                memberloginrecordService.SaveChanges();
                membersService.SaveChanges();
            }
            return this.Json("Success");
        }
    }
}