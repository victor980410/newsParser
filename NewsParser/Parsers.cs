﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Timers;
using HtmlAgilityPack;

namespace NewsParser
{
    struct NewsStructure
    {
        public string Title { get; set; }
        public string Text { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public string SourceUrl { get; set; }
        public DateTime TimeSourcePublished { get; set; }
    }

    interface IndexWebsiteParser
    {
        void ReadAllNewPages();
    }

    class EutroIntegrationParser : IndexWebsiteParser
    {
        const string websiteUrl = @"https://www.eurointegration.com.ua/news/1970/01/01/";

        private int currentPage = 7103300;
        private DataBaseConnector database = new DataBaseConnector("eurointegration");

        public void ReadAllNewPages()
        {
            try
            {
                while (true)
                {
                    var page = parsePage(currentPage);
                    Task task = Task.Run(async () => await database.InsertRecordAsync(page));
                    Console.WriteLine("Page " + currentPage.ToString() + " successufully read.");
                    currentPage++;
                }
            }
            catch (Exception ex)
            {
                if (CheckNextPage())
                {
                    currentPage++;
                    ReadAllNewPages();
                    return;
                }
                else
                {
                    Console.WriteLine(ex.Message);
                    return;
                }
            }
        }

        private bool CheckNextPage()
        {
            try
            {
                parsePage(currentPage + 1);
                return true;
            }
            catch (Exception)
            {
                return false;
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

            var tagsText = tags != null ? tags.Select(a => a.SelectSingleNode(".//a").InnerHtml) : null;
            var time = doc.DocumentNode.SelectSingleNode("//div[contains(@class,'post__time')]").InnerHtml;
            var dateTime = DateTime.Parse(time, System.Globalization.CultureInfo.GetCultureInfo("uk-UA"));

            return new NewsStructure
            {
                Title = title,
                Text = text,
                Tags = tagsText,
                TimeSourcePublished = dateTime,
                SourceUrl = websiteUrl + index.ToString()
            };
        }
    }
}
