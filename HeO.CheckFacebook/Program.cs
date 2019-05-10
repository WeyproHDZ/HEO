using OpenQA.Selenium;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Firefox;
using System.Web.Http;
using System.Web.Http.SelfHost;

namespace HeO.CheckFacebook
{
    class Program
    {
        static void Main(string[] args)
        {
            var config = new HttpSelfHostConfiguration("http://localhost:8080");
            config.Routes.MapHttpRoute("API", "{controller}/{action}/{id}",
                                        new { id = RouteParameter.Optional });
            //設定Self-Host Server，由於會使用到網路資源，用using確保會Dispose()加以釋放
            using (HttpSelfHostServer server = new HttpSelfHostServer(config))
            {
                using (var httpServer = new HttpSelfHostServer(config))
                {
                    //OpenAsync()屬非同步呼叫，加上Wait()則等待開啟完成才往下執行
                    httpServer.OpenAsync().Wait();
                    Console.WriteLine("Web API host started...");
                    //輸入exit按Enter結束httpServer
                    string line = null;
                    do
                    {
                        line = Console.ReadLine();
                    }
                    while (line != "exit");
                    //結束連線
                    httpServer.CloseAsync().Wait();
                }
            }
        }
    }
}
