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
using System.Net.NetworkInformation;
using Newtonsoft.Json;

namespace HeO.CheckFacebook
{
    public class CheckController : ApiController
    {
        private UseragentService useragentService;
        private MembersService membersService;

        public CheckController()
        {
            useragentService = new UseragentService();
            membersService = new MembersService();
        }
        [HttpGet]
        public string CheckFacebook(string Account, string Password, string Useragent)
        {
            Members member = membersService.Get().Where(a => a.Account == Account).FirstOrDefault();            
            string[] user_agent = new string[4];
            string FB_Account = Convert.ToString(Account);
            string[] status = new string[5];
            status[1] = "";
            status[2] = "";
            status[3] = "";
            status[4] = "";
            string api_useragent = Useragent.Replace("$", "/").Replace("*", " ");
            if (member != null)
            {
                string FacebookCookieJson = member.Facebookcookie;      // 撈該會員資料庫裡的FacebookCookie欄位資料
                FacebookCookieJson = FacebookCookieJson.Replace(@"\", "'");     // 將\ 取代成 '
                var FacebookCookieObject = JsonConvert.DeserializeObject<dynamic>(FacebookCookieJson); // FacebookCookieJson的json格式轉成物件
 
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
                Cookie cookie;
                DateTime date;
                foreach (var Object in FacebookCookieObject)
                {
                    if (Object.Expiry.ToString() != "")
                    {
                        date = Convert.ToDateTime(Object.Expiry);
                        if (date.ToString() != "")
                        {
                            cookie = new Cookie(Object.Name.ToString(), Object.Value.ToString(), Object.Domain.ToString(), Object.Path.ToString(), Convert.ToDateTime(Object.Expiry.ToString()));
                            driver.Manage().Cookies.AddCookie(cookie);
                        }
                    }
                }
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
                            var id_url = driver.FindElement(By.ClassName("profilePicThumb"));
                            var id = id_url.GetAttribute("href").Split('=');
                            var img = driver.FindElement(By.ClassName("_11kf")).GetAttribute("src");
                            var name = driver.Title.Split(')');
                            status[0] = "成功登入!";
                            status[1] = id.Last();
                            status[2] = img;
                            status[3] = name.Last();
                            /*** 登出 ***/
                            IWebElement Logout = driver.FindElement(By.XPath("//div[@id='logoutMenu']"));
                            Logout.Click();
                            System.Threading.Thread.Sleep(Delay());
                            IWebElement LogoutCheck = driver.FindElement(By.XPath("//div[@class='uiScrollableAreaContent']//li//*[text()='登出']"));
                            //IWebElement LogoutCheck = driver.FindElement(By.XPath("//li[8]/a/span/span"));                  
                            LogoutCheck.Click();
                            status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                            System.Threading.Thread.Sleep(Delay());
                            driver.Quit();
                        }
                    }
                    catch
                    {
                        status[0] = "請再輸入一次!";
                        var error = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                        driver.Quit();
                        return error;
                    }
                }
                var response = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];


                System.Threading.Thread.Sleep(500);
                driver.Quit();
                return response;
            }
            else
            {                
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
                            var id_url = driver.FindElement(By.ClassName("profilePicThumb"));
                            var id = id_url.GetAttribute("href").Split('=');
                            var img = driver.FindElement(By.ClassName("_11kf")).GetAttribute("src");
                            var name = driver.Title.Split(')');
                            status[0] = "成功登入!";
                            status[1] = id.Last();
                            status[2] = img;
                            status[3] = name.Last();
                            /*** 登出 ***/
                            IWebElement Logout = driver.FindElement(By.XPath("//div[@id='logoutMenu']"));
                            Logout.Click();
                            System.Threading.Thread.Sleep(Delay());
                            IWebElement LogoutCheck = driver.FindElement(By.XPath("//div[@class='uiScrollableAreaContent']//li//*[text()='登出']"));
                            //IWebElement LogoutCheck = driver.FindElement(By.XPath("//li[8]/a/span/span"));                  
                            LogoutCheck.Click();
                            status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                            System.Threading.Thread.Sleep(Delay());
                            driver.Quit();
                        }
                    }
                    catch
                    {
                        status[0] = "請再輸入一次!";
                        var error = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                        driver.Quit();
                        return error;
                    }
                }
                var response = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];


                System.Threading.Thread.Sleep(500);
                driver.Quit();
                return response;
            }
        }

        [HttpGet]
        public string BackendCkeckFacebook(string Facebooklink)
        {
            string status = "";
            FirefoxProfile profile = new FirefoxProfile();
            FirefoxOptions options = new FirefoxOptions();
            options.Profile = profile;
            options.SetPreference("dom.webnotifications.enabled", false);
            IWebDriver driver = new FirefoxDriver(options);
            driver.Navigate().GoToUrl(Facebooklink);
            if (driver.Title.IndexOf("找不到網頁") != -1)
            {
                status = "需驗證";
            }
            else
            {
                status = "已驗證";
            }
            driver.Quit();
            return status;
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

    public class JsontoObjectList
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string Domain { get; set; }
        public string Path { get; set; }
        public DateTime Expiry { get; set; }
    }
}