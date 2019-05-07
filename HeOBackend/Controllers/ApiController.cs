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

        [HttpGet]
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
                return this.Json("Success", JsonRequestBehavior.AllowGet);
            }
            else
            {
                return this.Json("Error", JsonRequestBehavior.AllowGet);
            }
        }

        [HttpGet]
        /*** 要帳密 ***/
        public JsonResult GetAccount(string Id, int number, string Ordernumber)
        {
            if (Id == "heo_order")
            {
                Order order = orderService.Get().Where(a => a.Ordernumber == Ordernumber).FirstOrDefault();
                Feedbackproduct feedbackproduct = feedbackproductService.Get().Where(a => a.Feedbackproductname.Contains(order.Service)).FirstOrDefault();
                Setting setting = settingService.Get().FirstOrDefault();                
                IEnumerable<Members> members = membersService.Get().Where(c => c.Isreal == true).Where(x => x.Lastdate <= DateTime.Now).OrderBy(a => a.Lastdate).Take(number);
                if(members.Count() == number)
                {
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

                        /*** 將會員寫到該訂單的互惠會員列表 ***/
                        Orderfaceooklist orderfacebooklist = new Orderfaceooklist();
                        orderfacebooklist.Memberid = thismembers.Memberid;
                        orderfacebooklist.Feedbackproductid = feedbackproduct.Feedbackproductid;
                        orderfacebooklist.Facebookaccount = thismembers.Account;
                        orderfacebooklist.Orderid = order.Orderid;
                        orderfacebooklist.Createdate = DateTime.Now;
                        orderfacebooklist.Updatedate = DateTime.Now;
                        orderfacebooklistService.Create(orderfacebooklist);

                        thismembers.Lastdate = thismembers.Lastdate.AddSeconds(Convert.ToDouble(setting.Time));
                        membersService.SpecificUpdate(thismembers, new string[] { "Lastdate" });
                    }
                    orderfacebooklistService.SaveChanges();
                    membersService.SaveChanges();
                    return this.Json(AccountList, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return this.Json("數量不足", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                string status = "error";
                return this.Json(status, JsonRequestBehavior.AllowGet);
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