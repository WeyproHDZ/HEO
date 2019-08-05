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

        public FeedbackMsController()
        {
            db = new HeOEntities();
            feedbackdetailService = new FeedbackdetailService();
            feedbackproductService = new FeedbackproductService();
            feedbackrecordService = new FeedbackrecordService();
            memberlevelService = new MemberlevelService();
            membersService = new MembersService();
        }
        // GET: FeedbackMs
        /**** 回饋金產品 新增/刪除/修改 ****/
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
                feedbackproductService.SaveChanges();
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
            return View(feedbackproduct);
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditFeedbackproduct(Guid Feedbackproductid, Feedbackproduct feedbackproduct)
        {
            if (TryUpdateModel(feedbackproduct, new string[] { "Feedbackproductname" }) && ModelState.IsValid)
            {
                feedbackproduct.Uuid = Guid.NewGuid();
                feedbackproduct.Updatedate = DateTime.Now;
                feedbackproductService.Update(feedbackproduct);
                feedbackproductService.SaveChanges();
            }
            return RedirectToAction("Feedbackproduct");
        }

        /**** 回饋金金額 新增/刪除/修改 ****/
        [CheckSession(IsAuth = true)]
        public ActionResult Feedbackdetail(int p = 1)
        {
            var data = feedbackproductService.Get().OrderBy(o => o.Createdate);
            ViewBag.pageNumber = p;
            ViewBag.Feedbackproduct = data.ToPagedList(pageNumber: p, pageSize: 20);

            return View();
        }

        [CheckSession(IsAuth = true)]
        public ActionResult AddFeedbackdetail()
        {
            IEnumerable<Feedbackproduct> feedbackproduct = feedbackproductService.Get();
            SelectList selectList = new SelectList(feedbackproduct, "feedbackproductid", "feedbackproductname");
            ViewBag.feedbackproductList = selectList;
            IEnumerable<Memberlevel> Levelname = memberlevelService.Get();
            ViewBag.Levelname = Levelname;
            return View();
        }
        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult AddFeedbackdetail(Guid feedbackproductid,Feedbackproduct feedbackproduct)
        {
            if (TryUpdateModel(feedbackproduct, new string[] { "Money" }) && ModelState.IsValid)
            {
                foreach (Feedbackdetail feedbackdetail in feedbackproduct.Feedbackdetail)
                {
                    feedbackdetail.Setid = Guid.NewGuid();
                    feedbackdetail.Feedbackproductid = feedbackproductid;
                    feedbackdetailService.Create(feedbackdetail);
                }
                feedbackdetailService.SaveChanges();

            }
           
            return RedirectToAction("Feedbackdetail");
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult DeleteFeedbackdetail(Guid Feedbackproductid , int p)
        {
            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(Feedbackproductid);
            foreach (Feedbackdetail fd in feedbackproduct.Feedbackdetail.ToArray())
            {
                feedbackdetailService.Delete(fd.Setid);
            }
            feedbackdetailService.SaveChanges();
            return RedirectToAction("Feedbackdetail", new { p = p });
        }

        [CheckSession(IsAuth = true)]
        [HttpGet]
        public ActionResult EditFeedbackdetail(Guid Feedbackproductid, int p)
        {
            ViewBag.pageNumber = p;

            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(Feedbackproductid);
            IEnumerable<Feedbackproduct> feedbackproduct_select = feedbackproductService.Get();
            SelectList selectList = new SelectList(feedbackproduct_select, "feedbackproductid", "feedbackproductname");
            IEnumerable<Memberlevel> Levelname = memberlevelService.Get();
            ViewBag.Levelname = Levelname;
            ViewBag.feedbackproductList = selectList;
            return View(feedbackproduct);
        }

        [CheckSession(IsAuth = true)]
        [HttpPost]
        public ActionResult EditFeedbackdetail(Guid Feedbackproductid,Feedbackproduct feedbackproduct_response)
        {
            Feedbackproduct feedbackproduct = feedbackproductService.GetByID(Feedbackproductid);

            foreach (Feedbackdetail feedbackproductdetail in feedbackproduct_response.Feedbackdetail)
            {
                //找尋資料庫有資料有資料-更新
                if (feedbackproduct.Feedbackdetail.ToList().Exists(x => x.Setid == feedbackproductdetail.Setid))
                {
                    Feedbackdetail fd = feedbackproduct.Feedbackdetail.Where(a => a.Setid == feedbackproductdetail.Setid).FirstOrDefault();
                    fd.Money = feedbackproductdetail.Money;
                    fd.Feedbackproductid = Feedbackproductid;
                }
                //找尋資料庫有資料無資料-新增
                else
                {
                    feedbackproductdetail.Setid = Guid.NewGuid();
                    feedbackproductdetail.Feedbackproductid = Feedbackproductid;
                    feedbackdetailService.Create(feedbackproductdetail);
                }
            }
            feedbackproductService.Update(feedbackproduct);
            feedbackproductService.SaveChanges();
            return RedirectToAction("Feedbackdetail", new { p = 1 });
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
                if (TryUpdateModel(feedbackrecord, new string[] { "Ishdz", "Money", "Status", "Hdzaccount", "Remark" }) && ModelState.IsValid)
                {
                    feedbackrecord.Updatedate = DateTime.Now;
                    feedbackrecord.Remains = old_total - feedbackrecord.Money;
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
            member.Feedbackmoney = member.Feedbackmoney + (member.Feedbackrecord.FirstOrDefault(a => a.Feedbackid == Feedbackid).Money - feedbackrecord.Money);
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