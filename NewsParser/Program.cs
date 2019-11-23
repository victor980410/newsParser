using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using System.Net;
using System.IO;
using System.Timers;

namespace NewsParser
{
    struct NewsStructure
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string SourceUrl { get; set; }
        public string TimeSourcePublished { get; set; }
    }

    interface IndexWebsiteParser
    {
        void ReadAllNewPages();
    }

    class EutroIntegrationParser : IndexWebsiteParser
    {
        const string websiteUrl = @"https://www.eurointegration.com.ua/news/1970/01/01/";

        private int currentPage = 7103410;
        private DataBaseConnector database = new DataBaseConnector("eurointegration");

        public void ReadAllNewPages()
        {
            try
            {
                while (true)
                {
                    var page = parsePage(currentPage);
                    Task task = Task.Run(async () => await database.InsertRecordAsync(page));
                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return;
            }
        }

        private NewsStructure parsePage(int index)
        {
            var targetUrl = new Uri(websiteUrl + index.ToString());
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
            var textTags = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'post__text')]").SelectNodes(".//p");
            var text = string.Join("\n", textTags.Select(t => t.InnerHtml));
            var tags = doc.DocumentNode.SelectNodes("//span[contains(@class,'post__tags__item')]");
            var tagsText = tags.Select(a => a.SelectSingleNode(".//a").InnerHtml);
            var time = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'post__time')]").InnerHtml;
            return new NewsStructure
            {
                Title = title,
                Text = text,
                Tags = tagsText,
                TimeSourcePublished = time,
                SourceUrl = websiteUrl + index.ToString()
            };
        }
    }

    class Program
    {
        static IndexWebsiteParser parser;
        static Timer timer;

        static void Main(string[] args)
        {
            parser = new EutroIntegrationParser();

            timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(Tick);
            timer.Interval = 3600000;

            GetNews();

            Console.ReadKey();
        }

        static void GetNews()
        {
            timer.Enabled = false;
            parser.ReadAllNewPages();
            timer.Enabled = true;
        }

        static void Tick(object source, ElapsedEventArgs e)
        {
            GetNews();
        }
    }
}
