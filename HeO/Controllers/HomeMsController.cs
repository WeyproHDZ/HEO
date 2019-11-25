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
using HeO.Libs;
using System.IO;
using System.Configuration;
using System.Diagnostics;
using System.Net.Http;
using System.Net;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using System.Web.Script.Serialization;

namespace HeO.Controllers
{
    public class HomeMsController : BaseController
    {
        private NewsService newsService;
        private NewslangService newslangService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;        
        private FeedbackproductService feedbackproductService;
        private MemberauthorizationService memberauthorizationService;
        private MemberloginrecordService memberloginrecordService;
        private UseragentService useragentService;
        public HomeMsController()
        {
            newsService = new NewsService();
            newslangService = new NewslangService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();           
            feedbackproductService = new FeedbackproductService();
            memberauthorizationService = new MemberauthorizationService();
            memberloginrecordService = new MemberloginrecordService();
            useragentService = new UseragentService();
        }

        // GET: HomeMs
        /*** 首頁 ***/
        public ActionResult Home()
        {
            ViewBag.News = newsService.Get().OrderByDescending(o => o.Createdate).Take(5);
            return View();
        }

        /*** 關於我們 ***/
        public ActionResult About()
        {
            return View();
        }

        /*** 登入畫面 ****/
        public ActionResult Login()
        {
            if (Convert.ToBoolean(Session["IsLogin"]))
            {
                return RedirectToAction("Home");
            }
            else
            {
                return View();
            }
        }
        [HttpPost]
        public ActionResult Login(Members members)
        {
            string Account = Regex.Replace(members.Account, @"[^a-z||A-Z||@||.||0-9||_]", "");         // 保留A-Z、a-z、0-9、小老鼠、小數點，其餘取代空值
            Members thismember = membersService.Get().Where(a => a.Account == members.Account).FirstOrDefault();
            string useragent_phone = "";
            if (thismember != null)
            {
                useragent_phone = thismember.Useragent_phone;
            }
            else
            {
                /***** useragent *****/
                useragent_phone = "Mozilla/5.0 (iPhone; CPU iPhone OS 11_1_2 like Mac OS X) AppleWebKit/604.3.5 (KHTML, like Gecko) Version/11.0 Mobile/15B202 Safari/604.1";
            }

            /**** HTTP POST ****/
            HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create("http://heohelp.com:8080/Check/CheckFacebook");
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "POST";

            using (var streamWriter = new StreamWriter(httpWebRequest.GetRequestStream()))
            {
                string json = new JavaScriptSerializer().Serialize(new
                {
                    Account = Account,
                    Password = members.Password,
                    Useragent = useragent_phone
                });

                streamWriter.Write(json);
            }

            HttpWebResponse httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            string result = "";
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }
            //ViewBag.message = result;
            //return View();
            string[] status = result.Replace("\"", "").Split('#');
            ///**** 測試用 ****/
            //string[] status = new string[5];
            //status[0] = "成功登入!";
            //status[1] = "";
            //status[2] = "";
            //status[3] = "";
            //status[4] = "";
            if (status[0] == "成功登入!")
            {
                Session["Img"] = status[2];
                Session["Facebookname"] = status[3];
                IEnumerable<Members> old_members = membersService.Get().ToList();
                Guid NormalLevelid = memberlevelService.Get().Where(a => a.Levelname == "一般").FirstOrDefault().Levelid;
                IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();                
                foreach (Members old_member in old_members)
                {
                    if (old_member.Facebookid.Equals(status[1]))
                    {                    
                        if (Session["href"] == null)
                        {
                            if (old_member.Facebookstatus == 0)
                            {
                                Session["IsLogin"] = true;
                                Session["Memberid"] = old_member.Memberid;

                                /**** 將會員成功登入寫進會員登入紀錄裡 ****/
                                Memberloginrecord memberloginrecord = new Memberloginrecord();
                                memberloginrecord.Memberid = old_member.Memberid;
                                memberloginrecord.Createdate = DateTime.Now;
                                memberloginrecord.Status = 1;
                                memberloginrecordService.Create(memberloginrecord);
                                memberloginrecordService.SaveChanges();
                                /**** End Memberloginrecord ****/
                                /**** 更新會員Facebooklink連結 *****/
                                old_member.Facebookid = status[1];
                                old_member.Facebookcookie = status[4];
                                old_member.Password = members.Password; // 更新密碼
                                old_member.Logindate = ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800);         // 登入時間為現在時間的總秒數
                                membersService.SpecificUpdate(old_member, new string[] { "Facebookid", "Facebookcookie", "Logindate", "Password" });
                                membersService.SaveChanges();
                                /***** End Facebookid ****/
                                return RedirectToAction("Certified");
                            }
                            else
                            {
                                Session["IsLogin"] = true;
                                Session["Memberid"] = old_member.Memberid;

                                /**** 將會員成功登入寫進會員登入紀錄裡 ****/
                                Memberloginrecord memberloginrecord = new Memberloginrecord();
                                memberloginrecord.Memberid = old_member.Memberid;
                                memberloginrecord.Createdate = DateTime.Now;
                                memberloginrecord.Status = 1;
                                memberloginrecordService.Create(memberloginrecord);
                                memberloginrecordService.SaveChanges();
                                /**** End Memberloginrecord ****/
                                /**** 更新會員Facebookid連結 *****/
                                old_member.Facebookid = status[1];
                                old_member.Facebookcookie = status[4];
                                old_member.Password = members.Password; // 更新密碼
                                old_member.Logindate = ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800);         // 登入時間為現在時間的總秒數
                                membersService.SpecificUpdate(old_member, new string[] { "Facebookid", "Facebookcookie", "Logindate", "Password" });
                                membersService.SaveChanges();
                                /***** End Facebookid ****/
                                return RedirectToAction("Order", "OrderMs");
                            }
                        }
                        else
                        {
                            Session["IsLogin"] = true;
                            Session["Memberid"] = old_member.Memberid;

                            /**** 將會員成功登入寫進會員登入紀錄裡 ****/
                            Memberloginrecord memberloginrecord = new Memberloginrecord();
                            memberloginrecord.Memberid = old_member.Memberid;
                            memberloginrecord.Createdate = DateTime.Now;
                            memberloginrecord.Status = 1;
                            memberloginrecordService.Create(memberloginrecord);
                            memberloginrecordService.SaveChanges();
                            /**** End Memberloginrecord ****/
                            /**** 更新會員Facebookid連結 *****/
                            old_member.Facebookid = status[1];
                            old_member.Facebookcookie = status[4];
                            old_member.Password = members.Password; // 更新密碼
                            old_member.Logindate = ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800);         // 登入時間為現在時間的總秒數
                            membersService.SpecificUpdate(old_member, new string[] { "Facebookid", "Facebookcookie", "Logindate", "Password" });
                            membersService.SaveChanges();
                            /***** End Facebookid ****/
                            return RedirectToAction("Deposit", "DepositMs");
                        }
                    }
                }
                if (TryUpdateModel(members, new string[] { "Password" }))
                {
                    /*** 隨機抓取Useragent ***/
                    int useragentCount = useragentService.Get().Count();
                    Useragent[] useragent = useragentService.Get().ToArray();
                    Random crand = new Random();
                    int rand = crand.Next(0, useragentCount - 1);                    
                    /******* 新增會員 ********/
                    members.Memberid = Guid.NewGuid();
                    members.Levelid = NormalLevelid;
                    members.Isenable = 1;                    
                    members.Is_import = 0;              // 是否匯入【0: 前台登入 , 1 : 後台匯入 , 2 : 轉前台】
                    members.Account = Account;
                    members.Createdate = DateTime.Now;
                    members.Updatedate = DateTime.Now;
                    members.Facebookcookie = status[4];
                    members.Useragent_phone = useragent[rand].User_agent;
                    members.Lastdate = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds -28800;
                    members.Logindate = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800;        // 紀錄目前登入時間
                    members.Name = status[3];
                    members.Facebookid = status[1];
                    /*** 預設將產品授權功能為fasle 【false:未授權 , true: 已授權】 ***/
                    foreach (Feedbackproduct feedbackproductlist in feedbackproduct)
                    {
                        Memberauthorization memberauthorization = new Memberauthorization();
                        memberauthorization.Id = Guid.NewGuid();
                        memberauthorization.Memberid = members.Memberid;
                        memberauthorization.Feedbackproductid = feedbackproductlist.Feedbackproductid;
                        memberauthorization.Checked = false;
                        members.Memberauthorization.Add(memberauthorization);

                        //memberauthorizationService.Create(memberauthorization);
                    }
                    /**** 將會員成功登入寫進會員登入紀錄裡 ****/
                    Memberloginrecord memberloginrecord = new Memberloginrecord();
                    memberloginrecord.Memberid = members.Memberid;
                    memberloginrecord.Createdate = members.Createdate;
                    memberloginrecord.Status = 1;
                    members.Memberloginrecord.Add(memberloginrecord);
                    /**** End Memberloginrecord ****/
                    membersService.Create(members);
                    membersService.SaveChanges();
                }

                Session["IsLogin"] = true;
                Session["Memberid"] = members.Memberid;
                if (Session["href"] == null)
                {
                    return RedirectToAction("Certified");
                }
                else
                {
                    return RedirectToAction("Deposit", "DepositMs");
                }
            }
            else
            {
                /*** 如果該會員有登過heo ***/
                if (thismember != null)
                {
                    /**** 將會員登入失敗寫進會員登入紀錄裡 ****/
                    Memberloginrecord memberloginrecord = new Memberloginrecord();
                    memberloginrecord.Memberid = thismember.Memberid;
                    memberloginrecord.Createdate = DateTime.Now;
                    memberloginrecord.Status = 2;
                    memberloginrecordService.Create(memberloginrecord);
                    memberloginrecordService.SaveChanges();
                    /**** End Memberloginrecord ****/
                }

                ViewBag.Status = status[0];              
                return View();
            }

        }

        [CheckSession]
        public ActionResult Logout()
        {            
            Members members = membersService.GetByID(Session["Memberid"]);
            /*** 登入時間改為現在 ****/
            members.Logindate = ((int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds - 28800);
            membersService.SpecificUpdate(members, new string[] { "Logindate" });
            membersService.SaveChanges();
            Session.RemoveAll();                                // 將所有Session清除
            return RedirectToAction("Home", "HomeMs");
        }

        [CheckSession]
        public ActionResult Certified()
        {
            return View();
        }
        [CheckSession]
        [HttpPost]
        public ActionResult Certified(int Facebookstatus)
        {
            Members member = membersService.GetByID(Session["Memberid"]);
            if(Facebookstatus != 0)
            {
                member.Facebookstatus = Facebookstatus;
                membersService.SpecificUpdate(member, new string[] { "Facebookstatus" });
                membersService.SaveChanges();
            }
            return RedirectToAction("Order", "OrderMs");
        }


        //[HttpPost]
        //public string CheckFacebook(string Account, string Password, string Useragent)
        //{
        //    return "account ="+Account+"password ="+Password+"useragent = "+Useragent;
        //}
     }
}