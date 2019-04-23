using System;
using System.Linq;
using System.Web;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Diagnostics;
using HeO.Models;
using HeO.Service;

namespace HeO.Libs
{
    public class Threadwork
    {
        public static string[] set_thread(IEnumerable<Order> order)
        {       
            Task[] task = new Task[(order.Count())];
            string[] response = new string[(order.Count() + 1)];
            int i = 0;
            foreach (Order orderList in order)
            {
                task[i] = Task.Factory.StartNew(() =>
                {                    
                    response[i] = "訂單名稱 : " + orderList.Ordernumber + "  訂單服務 : " + orderList.Service + "  需求數量 : " + orderList.Count;
                    i++;
                });
                System.Threading.Thread.Sleep(3000);
            }

            Task.Factory.ContinueWhenAll(task, copletedTasks =>
            {
                response[i] = "排程完成";
            });
            return response;
        }

        //public static string[] test2()
        //{
        //    string[] response = new string[4];
        //    var task1 = Task.Factory.StartNew(() =>
        //    {
        //        Thread.Sleep(3000);
        //        System.Diagnostics.Debug.WriteLine("Done!(3s)");
        //        response[0] = "Done!(3s)";                
        //    });
        //    var task2 = Task.Factory.StartNew(() =>
        //    {
        //        Thread.Sleep(5000);
        //        System.Diagnostics.Debug.WriteLine("Done!(5s)");
        //        response[2] = "Done!(5s)";
        //    });
        //    //等待任一作業完成後繼續
        //    Task.WaitAny(task1, task2);
        //    System.Diagnostics.Debug.WriteLine("WaitAny Passed");
        //    response[1] = "WaitAny Passed";
        //    //等待兩項作業都完成才會繼續執行
        //    Task.WaitAll(task1, task2);
        //    System.Diagnostics.Debug.WriteLine("WaitAll Passed");
        //    response[3] = "WaitAll Passed";
        //    return response;
        //}
    }
}