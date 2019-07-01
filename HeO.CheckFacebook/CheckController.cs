using OpenQA.Selenium;
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
            string FB_Account = Convert.ToString(Account);
            string[] status = new string[4];
            status[1] = "";
            status[2] = "";
            status[3] = "";
            string api_useragent = Useragent.Replace("$", "/").Replace("*", " ");
            FirefoxProfile profile = new FirefoxProfile();
            /*** 設定proxy ***/
            //profile.SetPreference("network.proxy.type", 1);
            //profile.SetPreference("network.proxy.http", "211.21.120.163");
            //profile.SetPreference("network.proxy.http_port", 8080);
            //profile.SetPreference("network.proxy.ssl", "211.23.221.121");
            //profile.SetPreference("network.proxy.ssl_port", 40135);
            //profile.SetPreference("network.proxy.socks", "123.205.179.16");
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
            /**** 放大螢幕 ****/
            driver.Manage().Window.Maximize();
            //Cookie cookie_presence = new Cookie("presence", "EDvF3EtimeF1559799076EuserFA21B24162243341A2EstateFDutF1559799076624CEchFDp_5f1B24162243341F2CC", ".facebook.com", "/", DateTime.Now.AddDays(1));
            //Cookie cookie_xsrc = new Cookie("x-src", "o8YBXSmhle8froY31VpscZSB", ".facebook.com", "/", Convert.ToDateTime("2021/06/12 03:44:40"));
            //Cookie cookie_datr = new Cookie("datr", "o8YBXSmhle8froY31VpscZSB", ".facebook.com", "/", DateTime.Now.AddHours(-12));
            //driver.Manage().Cookies.AddCookie(cookie_xsrc);
            //driver.Manage().Cookies.AddCookie(cookie_datr);         
            /*** 輸入帳號 ***/
            IWebElement FB_account = driver.FindElement(By.Id("email"));
            FB_account.Click();
            FB_account.SendKeys(FB_Account);
            System.Threading.Thread.Sleep(Delay());
            /*** 輸入密碼 ****/
            IWebElement FB_password = driver.FindElement(By.Id("pass"));
            FB_password.Click();
            FB_password.SendKeys(Password);
            System.Threading.Thread.Sleep(Delay());
            /*** 登入按鈕 ***/
            IWebElement SubmitButton = driver.FindElement(By.Id("loginbutton"));
            System.Threading.Thread.Sleep(Delay());
            SubmitButton.Click();
            System.Threading.Thread.Sleep(Delay());
            /*** 關閉網頁 ***/
            if (driver.Url.IndexOf("login") != -1)
            {
                status[0] = "帳號密碼有誤!";
                driver.Quit();
            }
            else if (driver.Url.IndexOf("checkpoint") != -1)
            {
                status[0] = "帳號未驗證!";
                /*** 登出 ***/
                driver.Navigate().GoToUrl("https://www.facebook.com/logout.php?h=Affi1hR_vyy5xR3E&t=1561684373&ref=mb");
                driver.Quit();
            }
            else
            {
                /***** 滑鼠滾輪 *****/
                IJavaScriptExecutor js = (IJavaScriptExecutor)driver;
                js.ExecuteScript("window.scrollTo(0," + Wheel() + ");");
                Delay();
                js.ExecuteScript("window.scrollTo(0," + Wheel() + ");");
                Delay();
                /*** 個人頁面 ****/
                IWebElement Profile = driver.FindElement(By.ClassName("linkWrap"));
                Profile.Click();
                System.Threading.Thread.Sleep(Delay());
                if (driver.Url.IndexOf("checkpoint") != -1)
                {
                    status[0] = "帳號未驗證!";
                    /*** 登出 ***/
                    driver.Navigate().GoToUrl("https://www.facebook.com/logout.php?h=Affi1hR_vyy5xR3E&t=1561684373&ref=mb");
                    driver.Quit();
                }
                try
                {
                    if (driver.Title.IndexOf("找不到網頁") != -1)
                    {
                        status[0] = "此帳號尚有其他防護，請解除後再登入!";
                        /*** 登出 ***/
                        driver.Navigate().GoToUrl("https://www.facebook.com/logout.php?h=Affi1hR_vyy5xR3E&t=1561684373&ref=mb");
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

                        /*** 登出 ***/
                        IWebElement Logout = driver.FindElement(By.XPath("//div[@id='logoutMenu']"));
                        Logout.Click();
                        System.Threading.Thread.Sleep(Delay());
                        IWebElement LogoutCheck = driver.FindElement(By.XPath("//div[@class='uiScrollableAreaContent']//li//*[text()='登出']"));
                        LogoutCheck.Click();
                        System.Threading.Thread.Sleep(Delay());
                        driver.Quit()
                        ;
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
        #region --延遲時間--
        private int Delay()
        {
            Random rnd = new Random();
            int delay_time = rnd.Next(3000, 5000);            // 亂數產生3秒至5秒
            return delay_time;
        }
        #endregion

        #region --滾輪座標--
        private int Wheel()
        {
            Random rnd = new Random();
            int wheel_coordinate = rnd.Next(100, 1080);
            return wheel_coordinate;
        }
        #endregion
    }
}