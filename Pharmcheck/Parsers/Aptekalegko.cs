﻿using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using AngleSharp.Text;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using static System.Net.Mime.MediaTypeNames;

namespace Pharmcheck.Parsers
{
    class Aptekalegko
    {
        public async static Task<IHtmlDocument> GetPage(string link)
        {
            HttpRequest request = new();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("Cache-Control", "max-age=0");
            request.AddHeader("Host", "aptekalegko.ru");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.AddHeader("Cookie", "tmr_lvid=6627d128e8c6ef92468b32a3521b4bea; tmr_lvidTS=1697615163363; _ym_uid=169761516324945722; _ym_d=1697615163; _ga=GA1.1.1716334610.1697615164; _ym_isad=1; _ym_visorc=w; c_guid=7771db27-19fb-4594-95c0-ff2e73ac727d; tmr_detect=1%7C1706715012112; _ga_MYLBR7F43E=GS1.1.1706713963.31.1.1706715016.0.0.0");
            request.KeepAlive = true;
            request.UserAgent = Http.ChromeUserAgent();
            string response = (await Task.Run(() => request.Get(link))).ToString();
            HtmlParser parser = new();
            IHtmlDocument page = parser.ParseDocument(response);
            return page;
        }

        public static int GetRequestStatus(IHtmlDocument page)
        {
            var statusCode = page.StatusCode;
            int status = Convert.ToInt32(statusCode);
            return status;
        }

        public static float GetPrice(IHtmlDocument page)
        {
            string price = page.QuerySelector("#__next > div._layout_main__HL_0j > main > div._wrapper_wrapper__AA1RI > div._productPage_colMain__5IcmH > div._productPage_row1__sdwUG > div._productPage_col__5XC0u > div > div > div > div > div._price_wrapperPrice__5dA7e > span").InnerHtml;
            if (price == "Нет в наличии") return 0;
            float result = Convert.ToSingle((price.Remove(price.Length - 2)).Replace('.',',')); //Удаление " ₽" и замена точки на запятую
            return result;
        }
        public static int GetShops(IHtmlDocument page)
        {
            string number = Regex.Replace(page.QuerySelector("#__next > div._layout_main__HL_0j > main > div._wrapper_wrapper__AA1RI > div._productPage_colMain__5IcmH > div._productPage_row1__sdwUG > div._productPage_col__5XC0u > div > div > div > a > p").TextContent, "[^0-9]", "");
            if (number == "") return 0;
            int shopsAmount = Convert.ToInt32(number);
            return shopsAmount;
        }
    }
}
