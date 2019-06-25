﻿using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;
using HeO.Models;
using HeO.Service;
using System.Web;

namespace HeO.CheckFacebook
{
    public class CheckController : ApiController
    {        
        private UseragentService useragentService;

        public CheckController()
        {
            useragentService = new UseragentService();
        }
        [HttpGet]
        public string CheckFacebook(string Account, string Password , string Useragent)
        {            
            string[] user_agent = new string[4];
            Random rnd = new Random();
            int num = rnd.Next(0, 2);
            int time = rnd.Next(1000, 3000);
            string FB_Account = Convert.ToString(Account);
            string[] status = new string[4];
            status[1] = "";
            status[2] = "";
            status[3] = "";
            string api_useragent = Useragent.Replace("$", "/").Replace("*", " ");
            FirefoxProfile profile = new FirefoxProfile();
            ///*** 設定proxy ***/
            //profile.SetPreference("network.proxy.type", 1);
            //profile.SetPreference("network.proxy.socks", "60.251.202.97");
            //profile.SetPreference("network.proxy.socks_port", 4145);
            /*** 設定useragent ***/
            profile.SetPreference("general.useragent.override", api_useragent);
            FirefoxOptions options = new FirefoxOptions();
            /*** 無痕 ****/
            options.AddArgument("--incognito");
            ///*** 無頭 ***/
            //options.AddArgument("--headless");
            //options.AddArgument("--disable-gpu");

            options.Profile = profile;            
            options.SetPreference("dom.webnotifications.enabled", false);
            IWebDriver driver = new FirefoxDriver(options);
            //driver.Navigate().GoToUrl("https://www.whatismyip.com.tw/");
            driver.Navigate().GoToUrl("https://www.facebook.com/");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1000);

            //Cookie cookie_presence = new Cookie("presence", "EDvF3EtimeF1559799076EuserFA21B24162243341A2EstateFDutF1559799076624CEchFDp_5f1B24162243341F2CC", ".facebook.com", "/", DateTime.Now.AddDays(1));
            //Cookie cookie_xsrc = new Cookie("x-src", "o8YBXSmhle8froY31VpscZSB", ".facebook.com", "/", Convert.ToDateTime("2021/06/12 03:44:40"));
            //Cookie cookie_datr = new Cookie("datr", "o8YBXSmhle8froY31VpscZSB", ".facebook.com", "/", DateTime.Now.AddHours(-12));
            //driver.Manage().Cookies.AddCookie(cookie_xsrc);
            //driver.Manage().Cookies.AddCookie(cookie_datr);
            //IEnumerable<Cookie> cookies = driver.Manage().Cookies.AllCookies.ToList();
            //status[0] = "";
            //status[1] = "";
            //status[2] = "";
            //status[3] = "";
            //var error = status[0] + "," + status[1] + "," + status[2] + "," + status[3] + "," + cookies;
            //driver.Quit();
            //return error;
            /*** 輸入帳號 ***/
            IWebElement FB_account = driver.FindElement(By.Id("email"));
            FB_account.Click();
            FB_account.SendKeys(FB_Account);
            System.Threading.Thread.Sleep(time);
            /*** 輸入密碼 ****/
            IWebElement FB_password = driver.FindElement(By.Id("pass"));
            FB_password.Click();
            FB_password.SendKeys(Password);
            System.Threading.Thread.Sleep(time);
            /*** 登入按鈕 ***/
            IWebElement SubmitButton = driver.FindElement(By.Id("loginbutton"));
            System.Threading.Thread.Sleep(time);
            SubmitButton.Click();
            System.Threading.Thread.Sleep(time);
            /*** 關閉網頁 ***/
            if (driver.Url.IndexOf("login") != -1)
            {
                status[0] = "帳號密碼有誤!";
                driver.Quit();
            }
            else if (driver.Url.IndexOf("checkpoint") != -1)
            {
                status[0] = "帳號未驗證!";
                driver.Quit();
            }
            else
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/profile.php");
                if (driver.Url.IndexOf("checkpoint") != -1)
                {
                    status[0] = "帳號未驗證!";
                    driver.Quit();
                }
                try
                {
                    if (driver.Title.IndexOf("找不到網頁") != -1)
                    {
                        status[0] = "此帳號尚有其他防護，請解除後再登入!";
                        driver.Quit();
                    }
                    else
                    {
                        status[0] = "成功登入!";
                        var id_url = driver.FindElement(By.ClassName("profilePicThumb"));
                        var id = id_url.GetAttribute("href").Split('=');

                        var img = driver.FindElement(By.ClassName("_11kf")).GetAttribute("src");
                        var name = driver.Title.Split(')');
                        status[1] = id.Last();
                        status[2] = img;
                        status[3] = name.Last();
                        driver.Quit();
                    }
                }
                catch
                {
                    status[0] = "請再輸入一次!";
                    status[1] = "";
                    status[2] = "";
                    status[3] = "";
                    var error = status[0] + "," + status[1] + "," + status[2] + "," + status[3];
                    driver.Quit();
                    return error;
                }
            }
            var response = status[0] + "," + status[1] + "," + status[2] + "," + status[3];


            System.Threading.Thread.Sleep(500);
            driver.Quit();
            return response;
        }
    }
}