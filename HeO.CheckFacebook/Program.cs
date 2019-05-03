using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using System.Diagnostics;
using System.IO;
using OpenQA.Selenium;

namespace HeO.CheckFacebook
{
    class Program
    {
        static string _Path = AppDomain.CurrentDomain.BaseDirectory;
        static string _Dir = "py";

        static void Main()
        {
            string filePath = _Path + _Dir + "\\" + "Check_fb.py";
            ProcessStartInfo start = new ProcessStartInfo();
            start.FileName = @"C:\Users\Jessie\AppData\Local\Programs\Python\Python37-32\python.exe";
            string Account = "git81685@cndps.com";
            string Password = "test001";
            start.Arguments = string.Format("{0} {1} {2}", filePath, Account, Password);

            start.UseShellExecute = false;
            start.RedirectStandardOutput = true;
            using (Process process = Process.Start(start))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    Console.WriteLine(result);
                }
            }
        }
    }
}
