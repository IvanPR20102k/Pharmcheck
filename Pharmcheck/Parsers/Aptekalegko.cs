using AngleSharp.Html.Parser;
using Leaf.xNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmcheck.Parsers
{
    class Aptekalegko
    {
        public static string GetPage(string link)
        {
            HttpRequest request = new();
            request.AddHeader(HttpHeader.Accept, "text/html,application/xhtml+xml,application/xml;q=0.9,image/avif,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.7");
            request.AddHeader("Accept-Encoding", "gzip, deflate");
            request.AddHeader("Accept-Language", "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7");
            request.AddHeader("Cache-Control", "max-age=0");
            request.AddHeader("Host", "aptekalegko.ru");
            request.AddHeader("Upgrade-Insecure-Requests", "1");
            request.KeepAlive = true;
            request.UserAgent = Http.ChromeUserAgent();
            string response = request.Get(link).ToString();
            return response;
        }

        public static string Parse(string response)
        {
            char[] chars = ['<', '>', '-', '₽'];
            HtmlParser parser = new();
            var doc = parser.ParseDocument(response);
            string price = "";
            price = doc.QuerySelector("#__next > div.main__NPU4nQ > main > div.wrapper__Xi6KzX > div.colMain__ISl2H7 > div.row1__BS5ACa > div.col__dJCsMk > div > div > div > div > div.wrapperPrice__MDVygx > span.span__fontSizeNormal__kZyjdi.text__LKKZWP.text__fontWeightBold__iXc3Vr.text__colorPrimary__eXXapd.undefined.price__PHToYi").InnerHtml;
            price = price.Remove(price.Length - 2);
            string result = $"{price}";
            return result;
        }
    }
}
