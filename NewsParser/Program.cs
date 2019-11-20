using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;

namespace NewsParser
{
    

    struct NewsStructure
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public string Tags { get; set; }
        public string SourceUrl { get; set; }
        public string TimeSourcePublished { get; set; }
    }

    interface IndexWebsiteParser
    {
        NewsStructure parsePage(int index);
    }

    class EutroIntegrationParser : IndexWebsiteParser
    {
        static string websiteUrl = @"https://www.eurointegration.com.ua/news/1970/01/01/";

        public NewsStructure parsePage(int index)
        {
            using (var wc = new WebClient())
            {
                wc.Headers.Set("Host", "www.eurointegration.com.ua");
                wc.Headers.Set("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36");
                wc.Headers.Set("accept", "text/html"); 
                wc.Headers.Set("accept-encoding", "deflate");
                wc.Headers.Set("accept-language", "ru-RU");


                HtmlDocument doc = new HtmlDocument();
                doc.LoadHtml(wc.DownloadString(websiteUrl + index.ToString()));
                var title = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'post__title')]").InnerHtml;
                var textTags = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'post__text')]").SelectNodes("//p");
                var text = string.Join("\n", textTags.Select(t => t.InnerHtml));
                return new NewsStructure
                {
                    Title = title,
                    Text = text,

                };
            }

       

            //var web = new HtmlWeb();
            //var doc = web.LoadHtml(websiteUrl + index.ToString());

        }

        public void hack()
        {
            var targetUrl = new Uri(websiteUrl + "7100000");
            var webReq = (HttpWebRequest)WebRequest.Create(targetUrl);
            webReq.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/78.0.3904.97 Safari/537.36";
            webReq.Accept = "text/html";
            webReq.Headers.Set("accept-encoding", "deflate");
            webReq.Headers.Set("accept-language", "ru-RU");

            WebResponse webRes = webReq.GetResponse();
            System.IO.Stream stream = webRes.GetResponseStream();
            System.Text.Encoding encode = System.Text.Encoding.GetEncoding("utf-8");
            System.IO.StreamReader reader = new System.IO.StreamReader(stream, Encoding.GetEncoding(1251));
            HtmlDocument doc = new HtmlDocument();
            doc.Load(reader);
            var title = doc.DocumentNode.SelectSingleNode("//h1[contains(@class,'post__title')]").InnerHtml;
            var textTags = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'post__text')]").SelectNodes("//p");
            var text = string.Join("\n", textTags.Select(t => t.InnerHtml));
        }
    }

    class Program
    {
        static void Main(string[] args)
        {

            EutroIntegrationParser parser = new EutroIntegrationParser();
            parser.hack();
            parser.parsePage(710000);
            // The code provided will print ‘Hello World’ to the console.
            // Press Ctrl+F5 (or go to Debug > Start Without Debugging) to run your app.
            Console.WriteLine("Hello World!");
            Console.ReadKey();

            // Go to http://aka.ms/dotnet-get-started-console to continue learning how to build a console app! 
        }
    }
}
