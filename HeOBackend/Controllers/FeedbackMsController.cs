using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagedList;
using HeO.Models;
using HeO.Service;

namespace HeOBackend.Controllers
{
    public class FeedbackMsController : BaseController
    {
        private HeOEntities db;
        private FeedbackdetailService feedbackdetailService;
        private FeedbackproductService feedbackproductService;
        private FeedbackrecordService feedbackrecordService;
        private MemberlevelService memberlevelService;
        private MembersService membersService;
        private MemberauthorizationService memberauthorizationService;
        public FeedbackMsController()
        {
            db = new HeOEntities();
            feedbackdetailService = new FeedbackdetailService();
            feedbackproductService = new FeedbackproductService();
            feedbackrecordService = new FeedbackrecordService();
            memberlevelService = new MemberlevelService();
            membersService = new MembersService();
            memberauthorizationService = new MemberauthorizationService();
        }
        // GET: FeedbackMs
        /**** 回饋金產品&價格 新增/刪除/修改 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Feedbackproduct(int p = 1)
        {
            var data = feedbackproductService.Get().OrderBy(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.Feedbackproduct = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddFeedbackproduct()
        {
            IEnumerable<Memberlevel> Levelname = memberlevelService.Get();
            ViewBag.Levelname = Levelname;
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddFeedbackproduct(Feedbackproduct feedbackproduct)
        {
            if (TryUpdateModel(feedbackproduct, new string[] { "Feedbackproductname" }) && ModelState.IsValid)
            {
                feedbackproduct.Feedbackproductid = Guid.NewGuid();
                feedbackproduct.Uuid = Guid.NewGuid();
                feedbackproduct.Createdate = DateTime.Now;
                feedbackproduct.Updatedate = DateTime.Now;
                feedbackproductService.Create(feedbackproduct);
                foreach (Feedbackdetail feedbackdetail in feedbackproduct.Feedbackdetail)
                {
                    feedbackdetail.Setid = Guid.NewGuid();
                    feedbackdetail.Feedbackproductid = feedbackproduct.Feedbackproductid;
                    feedbackdetailService.Create(feedbackdetail);
                }            
                feedbackproductService.SaveChanges();

                IEnumerable<Members> member = membersService.Get();
                foreach (Members thismember in member)
                {
                    Memberauthorization memberauth = new Memberauthorization();
                    memberauth.Id = Guid.NewGuid();
                    memberauth.Memberid = thismember.Memberid;
                    memberauth.Feedbackproductid = feedbackproduct.Feedbackproductid;
                    memberauth.Checked = true;
                    memberauthorizationService.Create(memberauth);
                }
                memberauthorizationService.SaveChanges();
            }
            return RedirectToAction("Feedbackproduct");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteFeedbackproduct(Guid productid , int p)
        {
            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(productid);

            feedbackproductService.Delete(feedbackproduct);
            feedbackproductService.SaveChanges();
            return RedirectToAction("Feedbackproduct", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditFeedbackproduct(Guid productid , int p)
        {
            ViewBag.pageNumber = p;
            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(productid);
            IEnumerable<Memberlevel> Levelname = memberlevelService.Get();
            ViewBag.Levelname = Levelname;
            return View(feedbackproduct);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditFeedbackproduct(Guid Feedbackproductid, ICollection<Feedbackdetail> Feedbackdetail)
        {
            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(Feedbackproductid);
            if (TryUpdateModel(feedbackproduct, new string[] { "Feedbackproductname" }) && ModelState.IsValid)
            {
                //更新產品價格
                foreach (Feedbackdetail new_feedbackdetail in Feedbackdetail)
                {
                    if(feedbackproduct.Feedbackdetail.ToList().Exists(a => a.Setid == new_feedbackdetail.Setid))
                    {
                        Feedbackdetail feedbackdetail = feedbackproduct.Feedbackdetail.Where(a => a.Setid == new_feedbackdetail.Setid).FirstOrDefault();
                        feedbackdetail.Money = new_feedbackdetail.Money;
                    }
                    else
                    {
                        new_feedbackdetail.Setid = Guid.NewGuid();
                        new_feedbackdetail.Feedbackproductid = Feedbackproductid;
                        feedbackproduct.Feedbackdetail.Add(new_feedbackdetail);
                    }                    
                }
                feedbackproductService.Update(feedbackproduct);
                feedbackproductService.SaveChanges();
            }
            return RedirectToAction("Feedbackproduct");
        }

        /**** 回饋金申請紀錄 新增/修改/刪除 ****/
        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult Feedbackrecord(string Account = "" , int p = 1)
        {
            /*** 下拉選單有填值 ****/
            if(Account != "")
            {
                IEnumerable<Feedbackrecord> data = feedbackrecordService.Get().Where(w => w.Members.Account.Contains(Account)).OrderByDescending(o => o.Createdate);

                IList<RecordTotal> total = db.Feedbackrecord.GroupBy(c => c.Memberid)
                            .Select(c => new RecordTotal
                            {
                                Memberid = c.Key,
                                Total = c.Sum(s => s.Money)
                            }).ToList();

                ViewBag.total = total;
                ViewBag.count = total.Count;
                ViewBag.pageNumber = p;
                ViewBag.Feedbackrecord = data.ToPagedList(pageNumber: p, pageSize: 20);
                //MembersDropDownList();
            }
            /**** 下拉選單沒填值 *****/
            else
            {
                IEnumerable<Feedbackrecord> data = feedbackrecordService.Get().OrderByDescending(o => o.Createdate);

                IList<RecordTotal> total = db.Feedbackrecord.GroupBy(c => c.Memberid)
                            .Select(c => new RecordTotal
                            {
                                Memberid = c.Key,
                                Total = c.Sum(s => s.Money)
                            }).ToList();

                ViewBag.total = total;
                ViewBag.count = total.Count;
                ViewBag.pageNumber = p;
                ViewBag.Feedbackrecord = data.ToPagedList(pageNumber: p, pageSize: 20);
                //MembersDropDownList();
            }

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddFeedbackrecord()
        {
            MembersDropDownList();
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddFeedbackrecord(Feedbackrecord feedbackrecord)
        {
            Members member = membersService.GetByID(feedbackrecord.Memberid);
            if(feedbackrecord.Ishdz == 0)
            {
                if (TryUpdateModel(feedbackrecord, new string[] {"Ishdz" , "Money", "Status", "Hdzaccount" , "Remark" }) && ModelState.IsValid)
                {
                    feedbackrecord.Feedbackid = Guid.NewGuid();
                    feedbackrecord.Createdate = DateTime.Now;
                    feedbackrecord.Updatedate = DateTime.Now;
                    feedbackrecord.Remains = member.Feedbackmoney - feedbackrecord.Money;
                    feedbackrecordService.Create(feedbackrecord);
                    feedbackrecordService.SaveChanges();
                }
            }
            else
            {
                if (TryUpdateModel(feedbackrecord, new string[] {"Ishdz" , "Money", "Status", "Bankaccount" , "Remark" }) && ModelState.IsValid)
                {
                    feedbackrecord.Feedbackid = Guid.NewGuid();
                    feedbackrecord.Createdate = DateTime.Now;
                    feedbackrecord.Updatedate = DateTime.Now;
                    feedbackrecord.Cash = feedbackrecord.Money / 2;
                    feedbackrecord.Remains = member.Feedbackmoney - feedbackrecord.Money;
                    feedbackrecordService.Create(feedbackrecord);
                    feedbackrecordService.SaveChanges();
                }
            }
            member.Feedbackmoney -= feedbackrecord.Money;
            membersService.SpecificUpdate(member, new string[] { "Feedbackmoney" });
            membersService.SaveChanges();
            return RedirectToAction("Feedbackrecord");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteFeedbackrecord(Guid Feedbackrecordid , int p)
        {
            Feedbackrecord feedbackrecord = feedbackrecordService.GetByID(Feedbackrecordid);

            feedbackrecordService.Delete(feedbackrecord);
            feedbackrecordService.SaveChanges();
            return RedirectToAction("Feedbackrecord", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditFeedbackrecord(Guid Feedbackrecordid , int p)
        {
            MembersDropDownList();
            ViewBag.pageNumber = p;
            Feedbackrecord feedbackrecord = feedbackrecordService.GetByID(Feedbackrecordid);
            return View(feedbackrecord);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditFeedbackrecord(Guid Feedbackid , Feedbackrecord feedbackrecord)
        {
            Members member = membersService.GetByID(feedbackrecord.Memberid);
            double old_total = member.Feedbackrecord.FirstOrDefault(a => a.Feedbackid == Feedbackid).Money + member.Feedbackrecord.FirstOrDefault(a => a.Feedbackid == Feedbackid).Remains;

            if (feedbackrecord.Ishdz == 0)
            {
                if(feedbackrecord.Status == 2)
                {
                    if (TryUpdateModel(feedbackrecord, new string[] { "Ishdz", "Money", "Status", "Hdzaccount", "Remark" }) && ModelState.IsValid)
                    {
                        feedbackrecord.Updatedate = DateTime.Now;
                        feedbackrecord.Remains = old_total + feedbackrecord.Money;
                        feedbackrecordService.Update(feedbackrecord);
                        feedbackrecordService.SaveChanges();
                    }
                }
                else
                {
                    if (TryUpdateModel(feedbackrecord, new string[] { "Ishdz", "Money", "Status", "Hdzaccount", "Remark" }) && ModelState.IsValid)
                    {
                        feedbackrecord.Updatedate = DateTime.Now;
                        feedbackrecord.Remains = old_total - feedbackrecord.Money;
                        feedbackrecordService.Update(feedbackrecord);
                        feedbackrecordService.SaveChanges();
                    }
                }
            }
            else
            {
                if (feedbackrecord.Status == 2)
                {
                    if (TryUpdateModel(feedbackrecord, new string[] { "Ishdz", "Money", "Status", "Bankaccount", "Remark" }) && ModelState.IsValid)
                    {
                        feedbackrecord.Updatedate = DateTime.Now;
                        feedbackrecord.Cash = feedbackrecord.Money / 2;
                        feedbackrecord.Remains = old_total + feedbackrecord.Money;
                        feedbackrecordService.Update(feedbackrecord);
                        feedbackrecordService.SaveChanges();
                    }
                }
                else
                {
                    if (TryUpdateModel(feedbackrecord, new string[] { "Ishdz", "Money", "Status", "Bankaccount", "Remark" }) && ModelState.IsValid)
                    {
                        feedbackrecord.Updatedate = DateTime.Now;
                        feedbackrecord.Cash = feedbackrecord.Money / 2;
                        feedbackrecord.Remains = old_total - feedbackrecord.Money;
                        feedbackrecordService.Update(feedbackrecord);
                        feedbackrecordService.SaveChanges();
                    }
                }

            }
            if(feedbackrecord.Status == 2)
            {
                member.Feedbackmoney = member.Feedbackmoney + feedbackrecord.Money;
            }
            else
            {
                member.Feedbackmoney = member.Feedbackmoney + (member.Feedbackrecord.FirstOrDefault(a => a.Feedbackid == Feedbackid).Money - feedbackrecord.Money);
            }
            membersService.SpecificUpdate(member, new string[] { "Feedbackmoney" });
            membersService.SaveChanges();
            return RedirectToAction("Feedbackrecord");
        }

        #region -- DropDownList ViewBag --
        private void MembersDropDownList(Object selectMember = null)
        {
            var querys = membersService.Get();

            ViewBag.Memberid = new SelectList(querys, "Memberid", "Account", selectMember);
        }
        #endregion
    }
}