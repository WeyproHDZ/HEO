using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;

namespace HeO.Libs
{

    public class CheckFacebook
    {
        static string[] status = new string[4];
        public static string[] Check_Facebook(string Account, string Password)
        {
            FirefoxOptions options = new FirefoxOptions();
            options.SetPreference("dom.webnotifications.enabled", false);
            IWebDriver driver = new FirefoxDriver(options);
            driver.Navigate().GoToUrl("https://www.facebook.com");
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(1000);
            /*** 輸入帳號 ***/
            IWebElement FB_account = driver.FindElement(By.Id("email"));
            FB_account.SendKeys(Account);
            System.Threading.Thread.Sleep(500);
            /*** 輸入密碼 ****/
            IWebElement FB_password = driver.FindElement(By.Id("pass"));
            FB_password.SendKeys(Password);
            System.Threading.Thread.Sleep(500);
            /*** 登入按鈕 ***/
            IWebElement SubmitButton = driver.FindElement(By.Id("loginbutton"));
            System.Threading.Thread.Sleep(500);
            SubmitButton.Click();
            System.Threading.Thread.Sleep(500);
            /*** 關閉網頁 ***/
            if(driver.Url.IndexOf("login") != -1)
            {
                status[0] = "帳號密碼有誤!";
                driver.Quit();
                return status;
            }
            else if (driver.Url.IndexOf("checkpoint") != -1)
            {
                status[0] = "帳號未驗證!";
                driver.Quit();
                return status;
            }
            else
            {
                driver.Navigate().GoToUrl("https://www.facebook.com/profile.php");
                if(driver.Title.IndexOf("找不到網頁") != -1)
                {
                    status[0] = "此帳號尚有其他防護，請解除後再登入!";
                    driver.Quit();
                    return status;
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
                    return status;
                }               
            }
        }
    }
}