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

namespace HeO.Controllers
{
    public class HomeMsController : BaseController
    {
        private NewsService newsService;
        private NewslangService newslangService;
        private MembersService membersService;
        private MemberlevelService memberlevelService;
        private MemberlevelcooldownService memberlevelcooldownService;
        private FeedbackproductService feedbackproductService;
        private MemberauthorizationService memberauthorizationService;
        private UseragentService useragentService;
        public HomeMsController()
        {
            newsService = new NewsService();
            newslangService = new NewslangService();
            membersService = new MembersService();
            memberlevelService = new MemberlevelService();
            memberlevelcooldownService = new MemberlevelcooldownService();
            feedbackproductService = new FeedbackproductService();
            memberauthorizationService = new MemberauthorizationService();
            useragentService = new UseragentService();
        }

        // GET: HomeMs
        /*** 首頁 ***/
        public ActionResult Home()
        {
            ViewBag.News = newsService.Get().Take(2).OrderBy(o => o.Createdate);
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
            Members thismember = membersService.Get().Where(a => a.Account == members.Account).FirstOrDefault();
            string useragent_com = "";
            string useragent_phone = "";
            if (thismember != null)
            {
                useragent_com = thismember.Useragent_com;
                useragent_phone = thismember.Useragent_phone;
            }
            else
            {
                int Now = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;                                                                          // 目前時間的總秒數

                if (Request.UserAgent.IndexOf("Windows") != -1)
                {
                    useragent_com = Request.UserAgent;
                }
                else if (Request.UserAgent.IndexOf("Macintosh") != -1)
                {
                    useragent_com = Request.UserAgent;
                }
                else if (Request.UserAgent.IndexOf("Linux") != -1)
                {
                    if (Request.UserAgent.IndexOf("Android") != -1)
                    {
                        useragent_phone = Request.UserAgent;
                    }
                    else
                    {
                        useragent_com = Request.UserAgent;
                    }
                }
                else
                {
                    useragent_phone = Request.UserAgent;
                }

                if (useragent_com == "")
                {
                    Useragent useragent = useragentService.Get().Where(a => a.Isweb == 0).Where(x => x.Date <= Now).OrderBy(x => x.Date).FirstOrDefault();
                    useragent.Date += 1800;
                    useragentService.SpecificUpdate(useragent, new string[] { "Date" });
                    useragentService.SaveChanges();
                    useragent_com = useragent.User_agent;
                }
                else
                {
                    Useragent useragent = useragentService.Get().Where(a => a.Isweb == 1).Where(x => x.Date <= Now).OrderBy(x => x.Date).FirstOrDefault();
                    useragent.Date += 1800;
                    useragentService.SpecificUpdate(useragent, new string[] { "Date" });
                    useragentService.SaveChanges();
                    useragent_phone = useragent.User_agent;
                }
            }

            string api_useragent = useragent_com.Replace(" ", "*").Replace("/", "$");
            string url = "http://heofrontend.4webdemo.com:8080/Check/CheckFacebook?Account=" + members.Account + "&Password=" + members.Password + "&Useragent=" + api_useragent;
            WebRequest myReq = WebRequest.Create(url);
            myReq.Method = "GET";
            myReq.ContentType = "application/json; charset=UTF-8";
            UTF8Encoding enc = new UTF8Encoding();
            myReq.Headers.Remove("auth-token");
            WebResponse wr = myReq.GetResponse();
            Stream receiveStream = wr.GetResponseStream();
            StreamReader reader = new StreamReader(receiveStream, Encoding.UTF8);
            string content = reader.ReadToEnd();
            string[] status = content.Replace("\"", "").Split(',');
            //string[] status = new string[4];
            //status[0] = "成功登入!";
            //status[1] = "";
            //status[2] = "";
            //status[3] = "";
            if (status[0] == "成功登入!")
            {
                Session["Img"] = status[2];
                Session["Facebookname"] = status[3];
                IEnumerable<Members> old_members = membersService.Get();
                Memberlevelcooldown memberlevelcooldown = memberlevelcooldownService.Get().OrderByDescending(o => o.Cooldowntime).FirstOrDefault();  // 撈會員層級冷卻時間最長的那筆資料
                IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();                
                foreach (Members old_member in old_members)
                {
                    if (members.Account == old_member.Account)
                    {
                        if (Session["href"] == null)
                        {
                            if (old_member.Facebookstatus == 0)
                            {
                                Session["IsLogin"] = true;
                                Session["Memberid"] = old_member.Memberid;
                                return RedirectToAction("Certified");
                            }
                            else
                            {
                                Session["IsLogin"] = true;
                                Session["Memberid"] = old_member.Memberid;
                                return RedirectToAction("Order", "OrderMs");
                            }
                        }
                        else
                        {
                            Session["IsLogin"] = true;
                            Session["Memberid"] = old_member.Memberid;
                            return RedirectToAction("Deposit", "DepositMs");
                        }
                    }
                }
                if (TryUpdateModel(members, new string[] { "Account", "Password" }))
                {
                    members.Memberid = Guid.NewGuid();
                    members.Levelid = memberlevelcooldown.Levelid;
                    members.Isenable = 1;
                    members.Createdate = DateTime.Now;
                    members.Updatedate = DateTime.Now;
                    members.Useragent_com = useragent_com;
                    members.Useragent_phone = useragent_phone;
                    members.Lastdate = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
                    members.Name = status[3];
                    members.Facebooklink = "https://www.facebook.com/profile.php?id=" + status[1];
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
                ViewBag.Status = status[0];
                return View();
            }

        }

        [CheckSession]
        public ActionResult Logout()
        {
            Session["IsLogin"] = false;
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


        [HttpPost]
        public string CheckFacebook(string Account, string Password, string Useragent)
        {
            return "account ="+Account+"password ="+Password+"useragent = "+Useragent;
        }
     }
}