using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HeO.Libs
{
    public class AllBankCode
    {
        public static string BankName(string BankCode)
        {
            string BankName;
            switch (BankCode)
            {
                case "BOT":
                    BankName = "台灣銀行";
                    break;
                case "LANDBANK":
                    BankName = "土地銀行";
                    break;
                case "TCB-BANK":
                    BankName = "合作金庫";
                    break;
                case "FIRSTBANK":
                    BankName = "第一銀行";
                    break;
                case "HNCB":
                    BankName = "華南銀行";
                    break;
                case "BANKCHB":
                    BankName = "彰化銀行";
                    break;
                case "SCSB":
                    BankName = "上海銀行";
                    break;
                case "FUBON":
                    BankName = "富邦銀行";
                    break;
                case "CATHAYBK":
                    BankName = "國泰世華";
                    break;
                case "BOK":
                    BankName = "高雄銀行";
                    break;
                case "MEGABANK":
                    BankName = "兆豐商銀";
                    break;
                case "CITIBANK":
                    BankName = "花旗銀行";
                    break;
                case "ANZ":
                    BankName = "澳盛銀行";
                    break;
                case "O-BANK":
                    BankName = "王道銀行";
                    break;
                case "TBB":
                    BankName = "台灣企銀";
                    break;
                case "SC":
                    BankName = "渣打銀行";
                    break;
                case "TCBBANK":
                    BankName = "台中銀行";
                    break;
                case "HSBC":
                    BankName = "滙豐銀行";
                    break;
                case "TAIPEISTARBANK":
                    BankName = "瑞興銀行";
                    break;
                case "HWATAIBANK":
                    BankName = "華泰銀行";
                    break;
                case "SKBANK":
                    BankName = "新光銀行";
                    break;
                case "SUNNYBANK":
                    BankName = "陽信銀行";
                    break;
                case "COTABANK":
                    BankName = "三信銀行";
                    break;
                case "POST":
                    BankName = "中華郵政";
                    break;
                case "UBOT":
                    BankName = "聯邦銀行";
                    break;
                case "FEIB":
                    BankName = "遠東銀行";
                    break;
                case "YUANTABANK":
                    BankName = "元大銀行";
                    break;
                case "SINOPAC":
                    BankName = "永豐銀行";
                    break;
                case "ESUNBANK":
                    BankName = "玉山銀行";
                    break;
                case "COSMOSBANK":
                    BankName = "凱基銀行";
                    break;
                case "DBS":
                    BankName = "星展銀行";
                    break;
                case "TAISHINBANK":
                    BankName = "台新銀行";
                    break;
                case "JIHSUNBANK":
                    BankName = "日盛銀行";
                    break;
                case "ENTIEBANK":
                    BankName = "安泰銀行";
                    break;
                case "CTBCBANK":
                    BankName = "中國信託";
                    break;
                default:
                    BankName = "無此銀行代碼";
                    break;


            }
            return BankName;
        }

        public static string Banknumber(string BankCode)
        {
            string Banknumber;
            switch (BankCode)
            {
                case "BOT":
                    Banknumber = "004";
                    break;
                case "LANDBANK":
                    Banknumber = "005";
                    break;
                case "TCB-BANK":
                    Banknumber = "006";
                    break;
                case "FIRSTBANK":
                    Banknumber = "007";
                    break;
                case "HNCB":
                    Banknumber = "008";
                    break;
                case "BANKCHB":
                    Banknumber = "009";
                    break;
                case "SCSB":
                    Banknumber = "011";
                    break;
                case "FUBON":
                    Banknumber = "012";
                    break;
                case "CATHAYBK":
                    Banknumber = "013";
                    break;
                case "BOK":
                    Banknumber = "016";
                    break;
                case "MEGABANK":
                    Banknumber = "017";
                    break;
                case "CITIBANK":
                    Banknumber = "021";
                    break;
                case "ANZ":
                    Banknumber = "039";
                    break;
                case "O-BANK":
                    Banknumber = "048";
                    break;
                case "TBB":
                    Banknumber = "050";
                    break;
                case "SC":
                    Banknumber = "052";
                    break;
                case "TCBBANK":
                    Banknumber = "053";
                    break;
                case "HSBC":
                    Banknumber = "081";
                    break;
                case "TAIPEISTARBANK":
                    Banknumber = "101";
                    break;
                case "HWATAIBANK":
                    Banknumber = "102";
                    break;
                case "SKBANK":
                    Banknumber = "103";
                    break;
                case "SUNNYBANK":
                    Banknumber = "108";
                    break;
                case "COTABANK":
                    Banknumber = "147";
                    break;
                case "POST":
                    Banknumber = "700";
                    break;
                case "UBOT":
                    Banknumber = "803";
                    break;
                case "FEIB":
                    Banknumber = "805";
                    break;
                case "YUANTABANK":
                    Banknumber = "806";
                    break;
                case "SINOPAC":
                    Banknumber = "807";
                    break;
                case "ESUNBANK":
                    Banknumber = "808";
                    break;
                case "COSMOSBANK":
                    Banknumber = "809";
                    break;
                case "DBS":
                    Banknumber = "810";
                    break;
                case "TAISHINBANK":
                    Banknumber = "812";
                    break;
                case "JIHSUNBANK":
                    Banknumber = "815";
                    break;
                case "ENTIEBANK":
                    Banknumber = "816";
                    break;
                case "CTBCBANK":
                    Banknumber = "822";
                    break;
                default:
                    Banknumber = "Error";
                    break;


            }
            return Banknumber;
        }
    }
}