using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using HeO.Models;
using HeO.Service;
namespace HeOBackend.Controllers
{
    public class ApiController : Controller
    {
        // GET: Api
        private OrderService orderService;
        private MembersService membersService;
        private SettingService settingService;
        private OrderfacebooklistService orderfacebooklistService;
        private FeedbackproductService feedbackproductService;
        public ApiController()
        {
            orderService = new OrderService();
            membersService = new MembersService();
            settingService = new SettingService();
            orderfacebooklistService = new OrderfacebooklistService();
            feedbackproductService = new FeedbackproductService();
        }

        [HttpGet]
        /*** 要訂單 ***/
        public JsonResult GetOrder(string Id)
        {
            if (Id == "heo_order")
            {
                Order order = orderService.Get().OrderBy(o => o.Createdate).FirstOrDefault();
                // 將訂單狀態改為進行中
                order.OrderStatus = 1;
                order.Service = order.Service;
                orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                orderService.SaveChanges();
                /*** 傳送訂單 ***/
                string[] OrderArray = new string[5];
                OrderArray[0] = order.Ordernumber;
                OrderArray[1] = order.Count.ToString();
                OrderArray[2] = order.Url;
                OrderArray[3] = order.Service;
                OrderArray[4] = order.OrderStatus.ToString();

                return this.Json(OrderArray, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string status = "error";
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        /**** 更新訂單 ***/
        public JsonResult UpdateOrder(string Id, string Ordernumber)
        {
            if(Ordernumber != null && Id == "heo_order")
            {
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();
                // 將訂單狀態改為完成中
                order.OrderStatus = 2;
                orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                orderService.SaveChanges();
                return this.Json("Success");
            }
            else
            {
                return this.Json("Error");
            }
        }

        [HttpGet]
        /*** 要帳密 ***/
        public JsonResult GetAccount(string Id, int number)
        {
            if (Id == "heo_order")
            {
                Setting setting = settingService.Get().FirstOrDefault();
                DateTime DateNow = DateTime.Now;
                DateTime NextuseDate = DateTime.Now.AddSeconds(Convert.ToDouble(setting.Time));
                IEnumerable<Members> members = membersService.Get().OrderBy(a => a.Lastdate).Where(x => x.Lastdate < DateNow).Where(c => c.Isreal == true).Take(number);
                List<get_member> AccountList = new List<get_member>();
                foreach (Members thismembers in members)
                {
                    AccountList.Add(
                        new get_member
                        {
                            memberid = thismembers.Memberid,
                            account = thismembers.Account,
                            password = thismembers.Password
                        }
                    );

                    thismembers.Lastdate = DateNow.AddSeconds(Convert.ToDouble(setting.Time));
                    membersService.SpecificUpdate(thismembers, new string[] { "Lastdate" });
                }
                membersService.SaveChanges();
                return this.Json(AccountList, JsonRequestBehavior.AllowGet);
            }
            else
            {
                string status = "error";
                return this.Json(status, JsonRequestBehavior.AllowGet);
            }
       }

        [HttpPost]
        /*** 更新帳戶 ***/
        public JsonResult UpdateAccount(string Id, string[] Account, string Ordernumber)
        {
            if(Id == "heo_order")
            {
                int i;
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(x => x.Feedbackproductname.Contains(order.Service)).FirstOrDefault();
                for (i = 0; i < Account.Length; i++)
                {
                    string thisAccount = Account[i];
                    Members members = membersService.Get().Where(a => a.Account == thisAccount).FirstOrDefault();
                    Orderfaceooklist orderfacebooklist = new Orderfaceooklist();
                    orderfacebooklist.Memberid = members.Memberid;
                    orderfacebooklist.Feedbackproductid = feedbackproduct.Feedbackproductid;
                    orderfacebooklist.Facebookaccount = members.Account;
                    orderfacebooklist.Orderid = order.Orderid;
                    orderfacebooklist.Createdate = DateTime.Now;
                    orderfacebooklist.Updatedate = DateTime.Now;
                    orderfacebooklistService.Create(orderfacebooklist);
                }
                orderfacebooklistService.SaveChanges();
                return this.Json("Success");
            }
            else
            {
                return this.Json("Error");
            }

        }
    }

    public class get_member
    {
        public Guid memberid { get; set; }
        public string account { get; set; }
        public string password { get; set; }
    }
}