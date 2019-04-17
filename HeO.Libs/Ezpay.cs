using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using HeO.Models;
using HeO.Service;
using System.Security.Cryptography;

namespace HeO.Libs
{
    public class Ezpay
    {
        static string transmit_url = "https://cpayment.ezpay.com.tw/MPG/mpg_gateway";
        static string MerchantID = "PG300000004533";
        static string HashKey = "quqMrp9Ijce4Mf0Kv29it8jIbFZGUYQS";
        static string HashIv = "yVfGbqv0QKKq5wkW";
        static string[] data = new string[9];
        public string[] paramer = new string[1000];
        public static void set_paramer(Viprecord viprecord, string CustomerURL , string Number = null)
        {            
            if(viprecord != null)
            {
                data[0] = MerchantID;
                data[1] = DateTime.Now.ToString();
                data[2] = "1.0";
                data[3] = viprecord.Depositnumber;
                data[4] = viprecord.Money.ToString();
                data[5] = "HeO 臉書互惠系統-儲值";
                data[6] = CustomerURL;
                data[7] = Number;
                data[8] = viprecord.Payway.ToString();
            }
        }
        
        public static string excute()
        {
            string TradeInfo = EncryptAES256(data);
            string TradeSha = getHashSha256("HashKey=" + HashKey + "&Amt=78&MerchantID = " + MerchantID + " & MerchantOrderNo =  & TimeStamp = 1450940783 & Version = 1.& HashIV = " + HashIv);
            string form =
            "<form id='ezpay' name='ezpay' action='" + transmit_url + "' method='post' >" +
            "<input type ='hidden' class = 'button-alt' name = 'MerchantID' value='" + MerchantID + "'/>" +
            "<input type ='hidden' class = 'button-alt' name = 'Version' value='" + data[2] + "'/>" +
            "<input type ='hidden' class = 'button-alt' name = 'TradeSha' value='" + TradeSha + "'/>" +
            "</form>"+
            "<script type='text/javascript'>document.getElementById('ezpay').submit();</script>";

            return form;     
        }
        public static string getHashSha256(string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);
            SHA256Managed hashstring = new SHA256Managed();
            byte[] hash = hashstring.ComputeHash(bytes);
            string hashString = string.Empty;
            foreach (byte x in hash)
            {
                hashString += String.Format("{0:x2}", x);
            }
            return hashString.ToUpper();
        }



        public string EncryptAES256(string source)//加密
        {
            string sSecretKey = "12345678901234567890123456789012";
            string iv = "1234567890123456";
            byte[] sourceBytes = AddPKCS7Padding(Encoding.UTF8.GetBytes(source),
           32);
            var aes = new RijndaelManaged();
            aes.Key = Encoding.UTF8.GetBytes(sSecretKey);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            ICryptoTransform transform = aes.CreateEncryptor();
            return ByteArrayToHex(transform.TransformFinalBlock(sourceBytes, 0,
           sourceBytes.Length)).ToLower();
        }
        public string DecryptAES256(string encryptData)//解密
        {
            string sSecretKey = "12345678901234567890123456789012";
            string iv = "1234567890123456";
            var encryptBytes = HexStringToByteArray(encryptData.ToUpper());
            var aes = new RijndaelManaged();
            aes.Key = Encoding.UTF8.GetBytes(sSecretKey);
            aes.IV = Encoding.UTF8.GetBytes(iv);
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            ICryptoTransform transform = aes.CreateDecryptor();
            return
           Encoding.UTF8.GetString(RemovePKCS7Padding(transform.TransformFinalBlock
           (encryptBytes, 0, encryptBytes.Length)));
        }
        private static byte[] AddPKCS7Padding(byte[] data, int iBlockSize)
        {
            int iLength = data.Length;
            byte cPadding = (byte)(iBlockSize - (iLength % iBlockSize));
            var output = new byte[iLength + cPadding];
            Buffer.BlockCopy(data, 0, output, 0, iLength);
            for (var i = iLength; i < output.Length; i++)
                output[i] = (byte)cPadding;
            return output;
        }
        private static byte[] RemovePKCS7Padding(byte[] data)
        {
            int iLength = data[data.Length - 1];
            var output = new byte[data.Length - iLength];
            Buffer.BlockCopy(data, 0, output, 0, output.Length);
            return output;
        }
        private static string ByteArrayToHex(byte[] barray)
        {
            char[] c = new char[barray.Length * 2];
            byte b;
            for (int i = 0; i < barray.Length; ++i)
            {
                b = ((byte)(barray[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);
                b = ((byte)(barray[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }
            return new string(c);
        }
        private static byte[] HexStringToByteArray(string hexString)
        {
         int hexStringLength = hexString.Length;
            byte[] b = new byte[hexStringLength / 2];
            for (int i = 0; i < hexStringLength; i += 2)
            {
                int topChar = (hexString[i] > 0x40 ? hexString[i] - 0x37 : hexString[i] -
               0x30) << 4;
                int bottomChar = hexString[i + 1] > 0x40 ? hexString[i + 1] - 0x37 :
               hexString[i + 1] - 0x30;
                b[i / 2] = Convert.ToByte(topChar + bottomChar);
            }
            return b;
        }



    }
}