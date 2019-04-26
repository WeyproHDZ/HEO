using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Libs;
using HeO.Models;
using HeO.Service;
using System.Data.Entity;

namespace HeO.Threadwork
{
    class Program
    {
        static OrderService orderService;
        static ThreadService threadService;

        static Program()
        {
            orderService = new OrderService();
            threadService = new ThreadService();
        }
        static void Main()
        {
            Thread thread = new Thread();
            // 判斷訂單裡有沒有正在進行中的資料
            Order old_order = orderService.Get().Where(a => a.OrderStatus == 1).OrderBy(o => o.Createdate).FirstOrDefault();
            if(old_order == null)
            {
                // 判斷訂單裡有沒有等待中的資料
                Order order = orderService.Get().Where(a => a.OrderStatus == 0).OrderBy(o => o.Createdate).FirstOrDefault();
                if(order != null)
                {
                    string response;
                    string OrderNumber = order.Ordernumber;

                    // 將訂單狀態改為進行中
                    order.OrderStatus = 1;
                    orderService.SpecificUpdate(order, new string[] { "OrderStatus" });
                    orderService.SaveChanges();

                    // 將此筆丟到排程裡
                    response = HeO.Libs.Threadwork.set_thread(order);
                    thread.Logs = response;
                    thread.Createdate = DateTime.Now;
                    threadService.Create(thread);
                    threadService.SaveChanges();

                    // 將訂單狀態改為完成中
                    Order thisorder = orderService.Get().Where(a => a.Ordernumber == OrderNumber).OrderBy(o => o.Createdate).FirstOrDefault();
                    thisorder.OrderStatus = 2;
                    orderService.SpecificUpdate(thisorder, new string[] { "OrderStatus" });
                    orderService.SaveChanges();
                }

            }
        }
    }
}
