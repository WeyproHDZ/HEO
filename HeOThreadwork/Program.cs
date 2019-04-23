using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HeO.Libs;
using HeO.Models;
using HeO.Service;

namespace HeOThreadwork
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
        static void Main(string[] args)
        {
            Thread thread = new Thread();
            IEnumerable<Order> order = orderService.Get().Where(a => a.OrderStatus == 0).OrderBy(o => o.Createdate);
            string[] response = new string[(order.Count())];
            response = Threadwork.set_thread(order);
            foreach (string thread_response in response)
            {
                thread.Logs = thread_response;
                thread.Createdate = DateTime.Now;
                threadService.Create(thread);
                threadService.SaveChanges();
            }
        }
    }
}