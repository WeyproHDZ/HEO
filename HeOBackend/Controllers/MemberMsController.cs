using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using System.IO;
using HeOBackend;
using HeO.Models;
using HeO.Service;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Data.OleDb;
using System.Data.SqlClient;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.Net;
using System.Text;

namespace HeOBackend.Controllers
{
    public class MemberMsController : BaseController
    {
        private HeOEntities db;
        private MemberlevelService memberlevelService;
        private MemberlevelcooldownService memberlevelcooldownService;
        private VipdetailService vipdetailService;
        private ViprecordService viprecordService;
        private MembersService membersService;
        private FeedbackproductService feedbackproductService;
        private MemberauthorizationService memberauthorizationService;
        private MemberloginrecordService memberloginrecordService;
        private UseragentService useragentService;

        private string fileSavedPath = WebConfigurationManager.AppSettings["UploadPath"];
        public MemberMsController()
        {
            db = new HeOEntities();
            memberlevelService = new MemberlevelService();
            memberlevelcooldownService = new MemberlevelcooldownService();
            vipdetailService = new VipdetailService();
            viprecordService = new ViprecordService();
            membersService = new MembersService();
            feedbackproductService = new FeedbackproductService();
            memberauthorizationService = new MemberauthorizationService();
            memberloginrecordService = new MemberloginrecordService();
            useragentService = new UseragentService();
        }
        // GET: MemberMs

        /**** 層級&各級別冷卻時間 新增/刪除/修改 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Memberlevel(int p = 1)
        {
            var data = memberlevelService.Get().OrderByDescending(a => a.Memberlevelcooldown.FirstOrDefault().Cooldowntime);
            ViewBag.pageNumber = p;
            ViewBag.Memberlevel = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddMemberlevel()
        {
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddMemberlevel(Memberlevel memberlevel , Memberlevelcooldown memberlevelcooldown)
        {
            if(TryUpdateModel(memberlevel, new string[] { "Levelname" }) && ModelState.IsValid)
            {
                memberlevel.Levelid = Guid.NewGuid();
                memberlevel.Isenable = 1;
                memberlevel.Createdate = DateTime.Now;
                memberlevel.Updatetime = DateTime.Now;
                memberlevelService.Create(memberlevel);
                memberlevelService.SaveChanges();
            }

            if(TryUpdateModel(memberlevelcooldown , new string[] { "Cooldowntime" }) && ModelState.IsValid)
            {
                memberlevelcooldown.Levelid = memberlevel.Levelid;
                memberlevelcooldownService.Create(memberlevelcooldown);
                memberlevelcooldownService.SaveChanges();
            }
            return RedirectToAction("Memberlevel");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteMemberlevel(Guid levelid, int p)
        {
            Memberlevel memebrlevel = memberlevelService.GetByID(levelid);

            memberlevelService.Delete(memebrlevel);
            memberlevelService.SaveChanges();

            return RedirectToAction("Memberlevel", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditMemberlevel(Guid levelid, int p)
        {
            ViewBag.pageNumber = p;
            Memberlevel level = memberlevelService.GetByID(levelid);
            return View(level);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditMemberlevel(Guid levelid, Memberlevel memberlevel , Memberlevelcooldown memberlevelcooldown)
        {
            if (TryUpdateModel(memberlevel, new string[] { "Levelname" }) && ModelState.IsValid)
            {
                memberlevel.Updatetime = DateTime.Now;
                memberlevelService.Update(memberlevel);
                memberlevelService.SaveChanges();
            }

            if (TryUpdateModel(memberlevelcooldown, new string[] { "Cooldowntime" }) && ModelState.IsValid)
            {
                memberlevelcooldownService.Update(memberlevelcooldown);
                memberlevelcooldownService.SaveChanges();
            }
            return RedirectToAction("Memberlevel");
        }

        /**** 會員 新增/刪除/修改/匯入Excel/驗證帳號 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Members(int p = 1)
        {
            var data = membersService.Get().OrderByDescending(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
            ViewBag.message = "按下確定後開始驗證帳號，請勿關閉分頁";
            LevelDropDownList("Members");
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult Members(Guid? Levelid , int p = 1 , string account = "" , string gender = "")
        {
            Guid RealLevelid = memberlevelService.Get().Where(a => a.Levelname == "真人").FirstOrDefault().Levelid;
            int genders;
            if (gender != "")
            {
                genders = Convert.ToInt16(gender);
            }
            /**** Account 、 Levelid有值  *****/
            if(account != "" && Levelid != null)
            {
                if (Levelid == RealLevelid)
                {
                    var data = membersService.Get().Where(x => x.Account.Contains(account)).Where(a => a.Isreal == true).OrderBy(o => o.Createdate);
                    ViewBag.pageNumber = p;
                    ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
                    ViewBag.Account = account;
                }
                else
                {
                    var data = membersService.Get().Where(x => x.Account.Contains(account)).Where(a => a.Levelid == Levelid).OrderBy(o => o.Createdate);
                    ViewBag.pageNumber = p;
                    ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
                    ViewBag.Account = account;
                }

            }
            /**** Levelid有值 *****/
            else if (Levelid != null && account == "")
            {
                if(Levelid == RealLevelid)
                {
                    var data = membersService.Get().Where(a => a.Isreal == true).OrderBy(o => o.Createdate);
                    ViewBag.pageNumber = p;
                    ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
                }
                else
                {
                    var data = membersService.Get().Where(a => a.Levelid == Levelid).OrderBy(o => o.Createdate);
                    ViewBag.pageNumber = p;
                    ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
                }
               
            }
            /**** Account有值 *****/
            else if (account != "" && Levelid == null)
            {
                var data = membersService.Get().Where(x => x.Account.Contains(account)).OrderBy(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
            }
            /**** Account 、 Levelid 、 Gender皆沒值 *****/
            else
            {
                var data = membersService.Get().OrderByDescending(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Members = data.ToPagedList(pageNumber: p, pageSize: 20);
            }
            LevelDropDownList("Members");

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddMembers()
        {
            IEnumerable<Members> members = membersService.Get();
            /**** 回饋金產品 *****/
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            ViewBag.feedbackproduct = feedbackproduct;
            /**** 等級選單 *****/
            LevelDropDownList();
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddMembers(Members members)
        {
            /*** 隨機指派手機版Useragent ***/
            int useragent_phone = useragentService.Get().Where(a => a.Isweb == 1).Count();
            Random rnd = new Random();
            int rnd_useragent = rnd.Next(1, useragent_phone);
            Useragent useragent = useragentService.Get().Where(a => a.Id == rnd_useragent).FirstOrDefault();
            /*** End Useragent ***/

            if (TryUpdateModel(members , new string[] {"Sex" , "Account" , "Password" , "Facebookstauts" , "Facebooklink" , "Feedbackmoney" , "Name" , "Isenable"}) && ModelState.IsValid)
            {
                members.Memberid = Guid.NewGuid();
                members.Account = Regex.Replace(members.Account, @"[^a-z||A-Z||@||.||0-9]", "");         // 保留A-Z、a-z、0-9、小老鼠、小數點，其餘取代空值
                members.Createdate = DateTime.Now;
                members.Updatedate = DateTime.Now;
                members.Lastdate = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;      // 總秒數
                members.Isenable = 1;
                members.Isreal = members.Isreal;
                members.Levelid = members.Levelid;              
                members.Useragent_phone = useragent.User_agent;
                /**** 將會員寫進會員登入紀錄裡，預設狀態為0 【0 : 未驗證 , 1 : 已驗證 , 2 : 需驗證】 ****/
                Memberloginrecord memberloginrecord = new Memberloginrecord();
                memberloginrecord.Memberid = members.Memberid;
                memberloginrecord.Createdate = members.Createdate;
                memberloginrecord.Status = 0;
                members.Memberloginrecord.Add(memberloginrecord);
                /**** End Memberloginrecord ****/
                membersService.Create(members);
                foreach (Memberauthorization memberauthorization in members.Memberauthorization)
                {
                    memberauthorization.Id = Guid.NewGuid();
                    memberauthorizationService.Create(memberauthorization);
                }
                membersService.SaveChanges();
            }

            return RedirectToAction("Members");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteMembers(Guid Memberid)
        {
            Members members = membersService.GetByID(Memberid);
            members.Isenable = 0;
            membersService.SpecificUpdate(members, new string[] { "Isenable" });
            membersService.SaveChanges();
            return RedirectToAction("Members");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditMembers(Guid Memberid , int p)
        { 
            ViewBag.pageNumber = p;
            Members member = membersService.GetByID(Memberid);
            LevelDropDownList(null,member);
            /**** 回饋金產品 *****/
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            ViewBag.feedbackproduct = feedbackproduct;
            return View(member);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditMembers(Guid Memberid , int p, ICollection<Memberauthorization> Memberauthorization)
        {
            Members member = membersService.GetByID(Memberid);
            if (TryUpdateModel(member, new string[] {"Sex" , "Account", "Levelid" , "Feedbackmoney" , "Name" , "Isreal"}) && ModelState.IsValid)
            {
                //更新權限新資料
                foreach (Memberauthorization new_auth in Memberauthorization)
                {
                    if (member.Memberauthorization.ToList().Exists(a => a.Id == new_auth.Id))
                    {
                        Memberauthorization member_auth = member.Memberauthorization.Where(a => a.Id == new_auth.Id).FirstOrDefault();
                        member_auth.Checked = new_auth.Checked;
                    }
                    else
                    {
                        new_auth.Id = Guid.NewGuid();
                        new_auth.Memberid = Memberid;
                        member.Memberauthorization.Add(new_auth);
                    }
                }
                member.Updatedate = DateTime.Now;                
                membersService.Update(member);
                membersService.SaveChanges();
            }
            
            return RedirectToAction("Members");
        }

        [HttpPost]
        [CheckSession(IsAuth =true)]
        public ActionResult UploadMembers(HttpPostedFileBase file)
        {
            if(file != null)
            {
                var filename = Path.GetFileName(file.FileName);
                var filetype = Path.GetExtension(file.FileName).ToLower();
                string newfileName = DateTime.Now.ToString("yyyyMMddHHmmssff") + filetype;
                var path = Path.Combine(Server.MapPath("~/FileUpload"), newfileName);
                file.SaveAs(path);
                importtoSQL(path);
            }
            return RedirectToAction("Members");
        }

        public void importtoSQL(string path)
        {
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get().ToList();
            IWorkbook workBook;
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.ReadWrite))
            {
                workBook = new XSSFWorkbook(fs);                
            }

            var sheet = workBook.GetSheet("工作表1");
            for (var i = 1; i <= sheet.LastRowNum; i++)
            {
                var j = sheet.LastRowNum;
                if (sheet.GetRow(i) != null)
                {
                    IEnumerable<Members> old_members = membersService.Get();        // 撈所有會員
                    Members member = new Members();
                    if(sheet.GetRow(i).GetCell(0).ToString() != "" && sheet.GetRow(i).GetCell(0).ToString() != null)
                    {
                        var test = sheet.GetRow(i).GetCell(3).ToString();
                        member.Account = Regex.Replace(sheet.GetRow(i).GetCell(0).ToString(), @"[^a-z||A-Z||@||.||0-9]", "");         // 保留A-Z、a-z、0-9、小老鼠、小數點，其餘取代空值
                        member.Password = sheet.GetRow(i).GetCell(1).ToString();
                        member.Name = sheet.GetRow(i).GetCell(2).ToString();
                        member.Facebooklink = sheet.GetRow(i).GetCell(3).ToString();
                        member.Levelid = Guid.Parse("0db21b54-35a7-400b-a8ea-e9c4c2879609");
                        member.Memberid = Guid.NewGuid();
                        member.Createdate = DateTime.Now;
                        member.Updatedate = DateTime.Now;
                        member.Isenable = 1;
                        member.Is_import = true;       // 是否匯入 【false : 前台登入 , true : 後台匯入】 
                        member.Lastdate = (int)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;
                        /*** 隨機指派手機版Useragent ***/
                        int useragent_phone = useragentService.Get().Where(a => a.Isweb == 1).Count();
                        Random rnd = new Random();
                        int rnd_useragent = rnd.Next(1, useragent_phone);
                        Useragent useragent = useragentService.Get().Where(a => a.Id == rnd_useragent).FirstOrDefault();
                        /*** End Useragent ***/
                        member.Useragent_phone = useragent.User_agent;

                        /**** 產品授權功能，預設都為true 【false : 未授權 , true: 已授權】 ****/
                        foreach (Feedbackproduct productList in feedbackproduct)
                        {
                            Memberauthorization memberauthorization = new Memberauthorization();
                            memberauthorization.Id = Guid.NewGuid();
                            memberauthorization.Memberid = member.Memberid;
                            memberauthorization.Feedbackproductid = productList.Feedbackproductid;
                            memberauthorization.Checked = true;
                            member.Memberauthorization.Add(memberauthorization);
                        }
                        /**** 將會員寫進會員登入紀錄裡，預設狀態為0 【0 : 未驗證 , 1 : 已驗證 , 2 : 需驗證】 ****/
                        Memberloginrecord memberloginrecord = new Memberloginrecord();
                        memberloginrecord.Memberid = member.Memberid;
                        memberloginrecord.Createdate = member.Createdate;
                        memberloginrecord.Status = 0;
                        member.Memberloginrecord.Add(memberloginrecord);
                        /**** End Memberloginrecord ****/
                        membersService.Create(member);
                    }                                      
                }               
            }
            membersService.SaveChanges();
        }

        /**** 驗證打勾跟問號會員 ***/
        [HttpGet]
        [CheckSession(IsAuth = true)]
        public ActionResult AuthMembers()
        {
            IEnumerable<Members> members = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status != 2).ToList();            
            foreach (Members auth_member in members)
            {
                string url = "http://heohelp.com:8080/Check/BackendCkeckFacebook?Facebooklink=" + auth_member.Facebooklink;
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
                if (content != "")          // 假設content不是空值時
                {
                    if (content == "已驗證")
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 1;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                    else
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 2;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                }
                else
                {
                    Memberloginrecord loginrecord = new Memberloginrecord();
                    loginrecord.Status = 0;
                    loginrecord.Memberid = auth_member.Memberid;
                    loginrecord.Createdate = DateTime.Now;
                    memberloginrecordService.Create(loginrecord);
                }

            }
            memberloginrecordService.SaveChanges();
            membersService.SaveChanges();
            TempData["message"] = "驗證已完成";
            return RedirectToAction("Members");
        }

        /*** 驗證全部會員 ***/
        [HttpGet]
        [CheckSession(IsAuth = true)]
        public ActionResult AllAuthMembers()
        {
            IEnumerable<Members> members = membersService.Get().ToList();
            foreach (Members auth_member in members)
            {
                string url = "http://heofrontend.4webdemo.com:8080/Check/BackendCkeckFacebook?Facebooklink=" + auth_member.Facebooklink;
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
                if(content != "")       // 假設content不是空值時
                {
                    if (content == "已驗證")
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 1;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                    else
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 2;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                }
                else
                {
                    Memberloginrecord loginrecord = new Memberloginrecord();
                    loginrecord.Status = 0;
                    loginrecord.Memberid = auth_member.Memberid;
                    loginrecord.Createdate = DateTime.Now;
                    memberloginrecordService.Create(loginrecord);
                }

            }
            memberloginrecordService.SaveChanges();
            membersService.SaveChanges();
            TempData["message"] = "驗證已完成";
            return RedirectToAction("Members");
        }

        /**** 驗證打叉會員 ***/
        [HttpGet]
        [CheckSession(IsAuth = true)]
        public ActionResult ErrorAuthMembers()
        {
            IEnumerable<Members> members = membersService.Get().Where(a => a.Memberloginrecord.OrderByDescending(x => x.Createdate).FirstOrDefault().Status == 2).ToList();
            foreach (Members auth_member in members)
            {
                string url = "http://heofrontend.4webdemo.com:8080/Check/BackendCkeckFacebook?Facebooklink=" + auth_member.Facebooklink;
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
                if(content != "")       // 假設content不是空值時
                {
                    if (content == "已驗證")
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 1;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                    else
                    {
                        Memberloginrecord loginrecord = new Memberloginrecord();
                        loginrecord.Status = 2;
                        loginrecord.Memberid = auth_member.Memberid;
                        loginrecord.Createdate = DateTime.Now;
                        memberloginrecordService.Create(loginrecord);
                    }
                }
                else
                {
                    Memberloginrecord loginrecord = new Memberloginrecord();
                    loginrecord.Status = 0;
                    loginrecord.Memberid = auth_member.Memberid;
                    loginrecord.Createdate = DateTime.Now;
                    memberloginrecordService.Create(loginrecord);
                }
            }
            memberloginrecordService.SaveChanges();
            membersService.SaveChanges();
            TempData["message"] = "驗證已完成";
            return RedirectToAction("Members");
        }
        /**** VIP費用設定 新增/刪除/修改 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Vipdetail(int p = 1)
        {
            var data = vipdetailService.Get().OrderBy(o => o.Money);
            ViewBag.pageNumber = p;
            ViewBag.Vipdetail = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddVipdetail()
        {
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddVipdetail(Vipdetail vipdetail)
        {
            if (TryUpdateModel(vipdetail, new string[] { "Day" , "Money" }) && ModelState.IsValid)
            {
                vipdetail.Vipdetailid = Guid.NewGuid();
                vipdetail.Createdate = DateTime.Now;
                vipdetail.Updatedate = DateTime.Now;
                vipdetailService.Create(vipdetail);
                vipdetailService.SaveChanges();
            }
            return RedirectToAction("Vipdetail");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteVipdetail(Guid Vipdetailid)
        {
            Vipdetail vipdetail = vipdetailService.GetByID(Vipdetailid);

            vipdetailService.Delete(vipdetail);
            vipdetailService.SaveChanges();
            return RedirectToAction("Vipdetail");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditVipdetail(Guid Vipdetailid, int p)
        {
            ViewBag.pageNumber = p;
            Vipdetail vipdetail = vipdetailService.GetByID(Vipdetailid);
            return View(vipdetail);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditVipdetail(Guid Vipdetailid, Vipdetail vipdetail)
        {
            if (TryUpdateModel(vipdetail, new string[] { "Day", "Money" }) && ModelState.IsValid)
            {
                vipdetail.Updatedate = DateTime.Now;
                vipdetailService.Update(vipdetail);
                vipdetailService.SaveChanges();
            }
            return RedirectToAction("Vipdetail");
        }

        /**** VIP購買紀錄 新增/刪除/修改 ****/
        //[HttpGet]
        //[CheckSession(IsAuth = true)]
        //public ActionResult Viprecord(int p = 1)
        //{
            
        //    //MembersDropDownList();
        //    var data = viprecordService.Get().OrderBy(o => o.Createdate);
        //    ViewBag.pageNumber = p;
        //    ViewBag.Viprecord = data.ToPagedList(pageNumber: p, pageSize: 20);
        //    return View();
        //}
        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult Viprecord(string Account = "", int p = 1)
        {
            if (Account != "")
            {
                //MembersDropDownList();
                var data = viprecordService.Get().Where(w => w.Members.Account.Contains(Account)).OrderByDescending(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Viprecord = data.ToPagedList(pageNumber: p, pageSize: 20);
            }
            else
            {
                //MembersDropDownList();
                var data = viprecordService.Get().OrderByDescending(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Viprecord = data.ToPagedList(pageNumber: p, pageSize: 20);
            }

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddViprecord()
        {
            MembersDropDownList();
            IEnumerable<Vipdetail> vipdetail = vipdetailService.Get().OrderByDescending(o => o.Day);
            SelectList VipdetailList = new SelectList(vipdetail, "Vipdetailid", "Day");
            ViewBag.VipdetailList = VipdetailList;
            return View();
        }
        [HttpPost]
        [CheckSession(IsAuth = true)]
        public ActionResult AddViprecord(Viprecord viprecord , Guid Vipdetailid)
        {
            Vipdetail vipdetail = vipdetailService.GetByID(Vipdetailid);
            Viprecord old_record = viprecordService.Get().OrderByDescending(o => o.Enddate).FirstOrDefault(a => a.Memberid == viprecord.Memberid);
            DateTime now = DateTime.Now;
            
            if (TryUpdateModel(viprecord, new string[] { "Payway" , "Status" }) && ModelState.IsValid)
            {
                viprecord.Viprecordid = Guid.NewGuid();
                viprecord.Createdate = DateTime.Now;
                viprecord.Updatedate = DateTime.Now;
                viprecord.Day = vipdetail.Day;
                viprecord.Money = vipdetail.Money;
                viprecord.Depositnumber = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                double day = Convert.ToDouble(viprecord.Day);
                /** 假設沒有舊資料，就直接新增進去 **/
                if(old_record != null)
                {
                    if (old_record.Enddate > now)
                    {
                        viprecord.Enddate = old_record.Enddate.AddDays(day);
                        viprecord.Startdate = old_record.Enddate;
                    }
                    else
                    {
                        viprecord.Enddate = DateTime.Now.AddDays(day);
                        viprecord.Startdate = DateTime.Now;
                    }
                }
                else
                {
                    viprecord.Enddate = DateTime.Now.AddDays(day);
                    viprecord.Startdate = DateTime.Now;
                }

                /*** 如果有完成付款，就將開始日期填入今天，填寫付款方式，並且將該會員之層級提升至VIP ***/
                if (viprecord.Status == 2)
                {
                    Members Member = membersService.GetByID(viprecord.Memberid);
                    Memberlevel Memberlevel = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault();
                    Member.Levelid = Memberlevel.Levelid;
                    Member.Feedbackmoney+= viprecord.Money;
                    membersService.SpecificUpdate(Member, new string[] { "Levelid" , "Feedbackmoney" });
                    membersService.SaveChanges();                   
                }
                viprecordService.Create(viprecord);
                viprecordService.SaveChanges();
            }

            return RedirectToAction("Viprecord");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteViprecord(Guid Viprecordid)
        {
            Viprecord viprecord = viprecordService.GetByID(Viprecordid);
            viprecordService.Delete(viprecord);
            viprecordService.SaveChanges();
            return RedirectToAction("Viprecord");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditViprecord(Guid Viprecordid, int p)
        {
            MembersDropDownList();
            IEnumerable<Vipdetail> vipdetail = vipdetailService.Get().OrderByDescending(o => o.Day);
            Viprecord viprecord = viprecordService.GetByID(Viprecordid);
            var selectday = vipdetailService.Get().FirstOrDefault(a => a.Day == viprecord.Day);
            SelectList VipdetailList = new SelectList(vipdetail, "Vipdetailid", "Day" , selectday.Vipdetailid);
            ViewBag.VipdetailList = VipdetailList;
            ViewBag.pageNumber = p;
            return View(viprecord);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditViprecord(Viprecord viprecord , Guid Vipdetailid)
        {
            Vipdetail vipdetail = vipdetailService.GetByID(Vipdetailid);
            if (TryUpdateModel(viprecord, new string[] { "Payway", "Status" }) && ModelState.IsValid)
            {
                viprecord.Updatedate = DateTime.Now;
                viprecord.Startdate = DateTime.Now;
                viprecord.Day = vipdetail.Day;
                viprecord.Money = vipdetail.Money;
                double day = Convert.ToDouble(viprecord.Day);
                viprecord.Enddate = DateTime.Now.AddDays(day);
                viprecordService.Update(viprecord);
                viprecordService.SaveChanges();
            }

            /*** 如果有完成付款，就將該會員之層級提升至VIP ***/
            if (viprecord.Status == 2)
            {
                Members Member = membersService.GetByID(viprecord.Memberid);
                Memberlevel Memberlevel = memberlevelService.Get().Where(a => a.Levelname == "VIP").FirstOrDefault();
                Member.Levelid = Memberlevel.Levelid;
                Member.Feedbackmoney += viprecord.Money;
                membersService.SpecificUpdate(Member, new string[] { "Levelid", "Feedbackmoney" });
                membersService.SaveChanges();
            }
            return RedirectToAction("Viprecord");
        }

        /**** 真人列表 刪除/修改/搜尋 *****/
        [CheckSession(IsAuth = true)]
        public ActionResult Reallist(int p = 1)
        {
            var data = membersService.Get().Where(a => a.Facebookstatus != 0).OrderByDescending(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.Reallist = data.ToPagedList(pageNumber: p, pageSize: 20);
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult Reallist(int p , string account = "")
        {
            var members = membersService.Get();
            if (account != "")
            {
                var data = membersService.Get().Where(a => a.Facebookstatus != 0).Where(x => x.Account.Contains(account)).OrderByDescending(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Reallist = data.ToPagedList(pageNumber: p, pageSize: 20);
                ViewBag.Account = account;
            }
            else
            {
                var data = membersService.Get().Where(a => a.Facebookstatus != 0).OrderByDescending(o => o.Createdate);
                ViewBag.pageNumber = p;
                ViewBag.Reallist = data.ToPagedList(pageNumber: p, pageSize: 20);
            }
            return View();
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditReallist(Guid memberid , int p)
        {
            ViewBag.pageNumber = p;
            Members members = membersService.GetByID(memberid);
            return View(members);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditReallist(Guid memberid , Members members , int status, Guid Levelid)
        {
            membersService.SpecificUpdate(members, new string[] { "Facebooklink" });
            members.Facebookstatus = status;
            if(status == 2)
            {
                members.Isreal = true;
                membersService.SpecificUpdate(members, new string[] { "Levelid" , "Isreal", "Facebookstatus" });
            }
            else
            {
                membersService.SpecificUpdate(members, new string[] { "Isreal", "Facebookstatus" });
            }
            membersService.SaveChanges();
            return RedirectToAction("Reallist");
        }

        #region -- LevelDropDownList ViewBag --
        private void LevelDropDownList(string Action = null,Object selectLevel = null)
        {
            if(Action == "Members")
            {
                var querys = memberlevelService.Get();

                ViewBag.Levelid = new SelectList(querys, "Levelid", "Levelname", selectLevel);
            }
            else
            {
                var querys = memberlevelService.Get().Where(a => a.Isenable == 1);

                ViewBag.Levelid = new SelectList(querys, "Levelid", "Levelname", selectLevel);
            }
          
        }
        #endregion

        #region -- MembersDropDownList ViewBag --
        private void MembersDropDownList(Object selectMember = null)
        {
            var querys = membersService.Get();

            ViewBag.Memberid = new SelectList(querys, "Memberid", "Account", selectMember);
        }
        #endregion
    }

}