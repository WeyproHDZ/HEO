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
            string[] user_agent = new string[4];
            string FB_Account = Convert.ToString(Account);
            string[] status = new string[5];
            status[1] = "";
            status[2] = "";
            status[3] = "";
            status[4] = "";
            string api_useragent = Useragent.Replace("$", "/").Replace("*", " ");
            int member = membersService.Get().Where(a => a.Account == Account).Count();
            if(member != 0)     // 判斷資料庫裡是否有這個會員的資料
            {
                Members members = membersService.Get().Where(a => a.Account == Account).FirstOrDefault();       // 撈該會員的詳細資料
                if (members.Facebookcookie != null)      // 判斷該會員在資料庫裡是否有Cookie資料
                {
                    string FacebookCookieJson = members.Facebookcookie;      // 撈該會員資料庫裡的FacebookCookie欄位資料
                    FacebookCookieJson = FacebookCookieJson.Replace(@"\", "'");     // 將\ 取代成 '
                    var FacebookCookieObject = JsonConvert.DeserializeObject<dynamic>(FacebookCookieJson); // FacebookCookieJson的json格式轉成物件
                    var content = "";
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
                    driver.Navigate().GoToUrl("https://mobile.facebook.com");
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
                    IWebElement FB_account = driver.FindElement(By.Id("m_login_email"));
                    FB_account.Click();
                    FB_account.SendKeys(FB_Account);
                    System.Threading.Thread.Sleep(Delay());
                    try
                    {
                        /*** 輸入密碼 ****/
                        IWebElement FB_password = driver.FindElement(By.Name("pass"));
                        FB_password.Click();
                        FB_password.SendKeys(Password);
                        System.Threading.Thread.Sleep(Delay());
                        /*** 登入按鈕 ***/
                        IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                        System.Threading.Thread.Sleep(Delay());
                        SubmitButton.Click();
                        System.Threading.Thread.Sleep(Delay());
                    }
                    catch
                    {
                        try
                        {
                            IWebElement FB_continue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/form/div[4]/div[1]/button"));
                            FB_continue.Click();
                            System.Threading.Thread.Sleep(Delay());
                            /*** 輸入密碼 ****/
                            IWebElement FB_password = driver.FindElement(By.Name("pass"));
                            FB_password.Click();
                            FB_password.SendKeys(Password);
                            System.Threading.Thread.Sleep(Delay());
                            /*** 登入按鈕 ***/
                            IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                            System.Threading.Thread.Sleep(Delay());
                            SubmitButton.Click();
                            System.Threading.Thread.Sleep(Delay());
                        }
                        catch
                        {
                            /*** 登入按鈕 ***/
                            IWebElement SubmitButton = driver.FindElement(By.Name("login_form"));
                            System.Threading.Thread.Sleep(Delay());
                            SubmitButton.Submit();
                            System.Threading.Thread.Sleep(Delay());
                        }
                    }
                    /**** 判斷唉呀，好像有東西出錯的錯誤訊息 ****/
                    if(driver.Url.IndexOf("error") != -1)
                    {
                        driver.Navigate().GoToUrl("https://mobile.facebook.com");
                        /*** 個人頁面 ****/

                        IWebElement Setting = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/a[1]"));
                        Setting.Click();
                        System.Threading.Thread.Sleep(Delay());
                        IWebElement Profile = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/a[1]/div[1]/img[1]"));
                        Profile.Click();
                        IWebElement img = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/a[1]/div[1]/i[1]"));
                        string[] imgdata = img.GetAttribute("style").Replace("\'", "\"").Split('\"');
                        string name = img.GetAttribute("aria-label");
                        status[0] = "成功登入!";
                        status[1] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                        status[2] = imgdata[1].Replace("amp;", "");
                        status[3] = name;
                        status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                        System.Threading.Thread.Sleep(Delay());
                        driver.Quit();
                    }
                    else
                    {
                        if (driver.Url.IndexOf("save-device") != -1)
                        {
                            /*** 稍後再用按鈕 ****/
                            IWebElement FB_continue = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[1]/a[1]"));
                            FB_continue.Click();
                            System.Threading.Thread.Sleep(Delay());
                            /*** 個人頁面 ****/

                            IWebElement Setting = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/a[1]"));
                            Setting.Click();
                            System.Threading.Thread.Sleep(Delay());
                            IWebElement Profile = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/a[1]/div[1]/img[1]"));
                            Profile.Click();
                            IWebElement img = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/a[1]/div[1]/i[1]"));
                            string[] imgdata = img.GetAttribute("style").Replace("\'", "\"").Split('\"');
                            string name = img.GetAttribute("aria-label");
                            status[0] = "成功登入!";
                            status[1] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                            status[2] = imgdata[1].Replace("amp;", "");
                            status[3] = name;
                            status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                            System.Threading.Thread.Sleep(Delay());
                            driver.Quit();
                        }
                        else
                        {
                            /*** 帳號需驗證 ***/
                            if (driver.Url.IndexOf("checkpoint") != -1)
                            {
                                status[0] = "帳號未驗證";
                                driver.Quit();
                                content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                                return content;
                            }
                            else if (driver.Url.IndexOf("save-device") == -1)
                            /*** 帳密輸入錯誤 ***/
                            {
                                status[0] = "帳號密碼有誤!";
                                driver.Quit();
                                content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                                return content;
                            }
                        }
                    }                                                               
                    var response = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                    System.Threading.Thread.Sleep(500);
                    driver.Quit();
                    return response;
                }
                else
                {
                    var content = "";
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
                    driver.Navigate().GoToUrl("https://mobile.facebook.com/");
                    driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1000);
                    /**** 放大螢幕 ****/
                    driver.Manage().Window.Maximize();
                    /*** 輸入帳號 ***/
                    IWebElement FB_account = driver.FindElement(By.Id("m_login_email"));
                    FB_account.Click();
                    FB_account.SendKeys(FB_Account);
                    System.Threading.Thread.Sleep(Delay());
                    try
                    {
                        /*** 輸入密碼 ****/
                        IWebElement FB_password = driver.FindElement(By.Name("pass"));
                        FB_password.Click();
                        FB_password.SendKeys(Password);
                        System.Threading.Thread.Sleep(Delay());
                        /*** 登入按鈕 ***/
                        IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                        System.Threading.Thread.Sleep(Delay());
                        SubmitButton.Click();
                        System.Threading.Thread.Sleep(Delay());
                    }
                    catch
                    {
                        try
                        {
                            IWebElement FB_continue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/form/div[4]/div[1]/button"));
                            FB_continue.Click();
                            System.Threading.Thread.Sleep(Delay());
                            /*** 輸入密碼 ****/
                            IWebElement FB_password = driver.FindElement(By.Name("pass"));
                            FB_password.Click();
                            FB_password.SendKeys(Password);
                            System.Threading.Thread.Sleep(Delay());
                            /*** 登入按鈕 ***/
                            IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                            System.Threading.Thread.Sleep(Delay());
                            SubmitButton.Click();
                            System.Threading.Thread.Sleep(Delay());
                        }
                        catch
                        {
                            /*** 登入按鈕 ***/
                            IWebElement SubmitButton = driver.FindElement(By.Name("login_form"));
                            System.Threading.Thread.Sleep(Delay());
                            SubmitButton.Submit();
                            System.Threading.Thread.Sleep(Delay());
                        }
                    }

                    if (driver.Url.IndexOf("save-device") != -1)
                    {
                        /*** 稍後再用按鈕 ****/
                        IWebElement FB_continue = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[1]/a[1]"));
                        FB_continue.Click();
                        System.Threading.Thread.Sleep(Delay());
                        /*** 個人頁面 ****/

                        IWebElement Setting = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/a[1]"));
                        Setting.Click();
                        System.Threading.Thread.Sleep(Delay());
                        IWebElement Profile = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/a[1]/div[1]/img[1]"));
                        Profile.Click();
                        IWebElement img = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/a[1]/div[1]/i[1]"));
                        string[] imgdata = img.GetAttribute("style").Replace("\'", "\"").Split('\"');
                        string name = img.GetAttribute("aria-label");
                        status[0] = "成功登入!";
                        status[1] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                        status[2] = imgdata[1].Replace("amp;", "");
                        status[3] = name;
                        status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                        System.Threading.Thread.Sleep(Delay());
                        driver.Quit();
                    }
                    else
                    {
                        /*** 帳號需驗證 ***/
                        if (driver.Url.IndexOf("checkpoint") != -1)
                        {
                            status[0] = "帳號未驗證";
                            driver.Quit();
                            content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                            return content;
                        }
                        else if (driver.Url.IndexOf("save-device") == -1)
                        /*** 帳密輸入錯誤 ***/
                        {
                            status[0] = "帳號密碼有誤!";
                            driver.Quit();
                            content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                            return content;
                        }
                    }

                    var response = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                    System.Threading.Thread.Sleep(500);
                    driver.Quit();
                    return response;
                }
            }
            /**** 資料庫裡沒有該會員的資料(該會員為第一次登入HEO的新會員) ****/
            else
            {
                var content = "";
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
                driver.Navigate().GoToUrl("https://mobile.facebook.com/");
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1000);
                /**** 放大螢幕 ****/
                driver.Manage().Window.Maximize();
                /*** 輸入帳號 ***/
                IWebElement FB_account = driver.FindElement(By.Id("m_login_email"));
                FB_account.Click();
                FB_account.SendKeys(FB_Account);
                System.Threading.Thread.Sleep(Delay());
                try
                {
                    /*** 輸入密碼 ****/
                    IWebElement FB_password = driver.FindElement(By.Name("pass"));
                    FB_password.Click();
                    FB_password.SendKeys(Password);
                    System.Threading.Thread.Sleep(Delay());
                    /*** 登入按鈕 ***/
                    IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                    System.Threading.Thread.Sleep(Delay());
                    SubmitButton.Click();
                    System.Threading.Thread.Sleep(Delay());
                }
                catch
                {
                    try
                    {
                        
                        IWebElement FB_continue = driver.FindElement(By.XPath("/html/body/div[1]/div/div[2]/div[2]/div/div[2]/div[2]/form/div[4]/div[1]/button"));
                        FB_continue.Click();
                        System.Threading.Thread.Sleep(Delay());
                        /*** 輸入密碼 ****/
                        IWebElement FB_password = driver.FindElement(By.Name("pass"));
                        FB_password.Click();
                        FB_password.SendKeys(Password);
                        System.Threading.Thread.Sleep(Delay());
                        /*** 登入按鈕 ***/
                        IWebElement SubmitButton = driver.FindElement(By.Name("login"));
                        System.Threading.Thread.Sleep(Delay());
                        SubmitButton.Click();
                        System.Threading.Thread.Sleep(Delay());
                    }
                    catch
                    {
                        /*** 登入按鈕 ***/
                        IWebElement SubmitButton = driver.FindElement(By.Name("login_form"));
                        System.Threading.Thread.Sleep(Delay());
                        SubmitButton.Submit();
                        System.Threading.Thread.Sleep(Delay());
                    }
                }
                if(driver.Url.IndexOf("save-device") != -1)
                {
                    /*** 稍後再用按鈕 ****/
                    IWebElement FB_continue = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[1]/div[1]/div[3]/div[1]/div[1]/div[1]/a[1]"));
                    FB_continue.Click();
                    System.Threading.Thread.Sleep(Delay());
                    /*** 個人頁面 ****/

                    IWebElement Setting = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/a[1]"));
                    Setting.Click();
                    System.Threading.Thread.Sleep(Delay());
                    IWebElement Profile = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[2]/div[1]/div[1]/div[6]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/div[1]/ul[1]/li[1]/div[1]/a[1]/div[1]/img[1]"));
                    Profile.Click();
                    IWebElement img = driver.FindElement(By.XPath("/html[1]/body[1]/div[1]/div[1]/div[4]/div[1]/div[1]/div[1]/div[1]/div[1]/div[2]/div[1]/a[1]/div[1]/i[1]"));
                    string[] imgdata = img.GetAttribute("style").Replace("\'", "\"").Split('\"');
                    string name = img.GetAttribute("aria-label");
                    status[0] = "成功登入!";
                    status[1] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                    status[2] = imgdata[1].Replace("amp;", "");
                    status[3] = name;
                    status[4] = Newtonsoft.Json.JsonConvert.SerializeObject(driver.Manage().Cookies.AllCookies);
                    System.Threading.Thread.Sleep(Delay());
                    driver.Quit();
                }
                else
                {
                    /*** 帳號需驗證 ***/
                    if (driver.Url.IndexOf("checkpoint") != -1)
                    {
                        status[0] = "帳號未驗證";
                        driver.Quit();
                        content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                        return content;
                    }
                    else if (driver.Url.IndexOf("save-device") == -1)
                    /*** 帳密輸入錯誤 ***/
                    {
                        status[0] = "帳號密碼有誤!";
                        driver.Quit();
                        content = status[0] + "#" + status[1] + "#" + status[2] + "#" + status[3] + "#" + status[4];
                        return content;
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