using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using HtmlAgilityPack;
using System.IO;
using System.Windows.Forms;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace IMDBScraper
{
    class ImdbEntry
    {
        public string ImdbId { get; private set; }
        public string Name { get; private set; }
        public double Rating { get; private set; }
        public int RatingCount { get;private set; }
        public int Year { get; set; }
        public IEnumerable<string> Genres { get; private set; }
        public ImdbEntry(string id)
        {
            // try 3 times, we are talking about internet....
            for (int i = 0; i < 3; i++)
            {
                try
                {
                    Genres = new string[0];
                    ImdbId = id;
                    var downloader = (HttpWebRequest)HttpWebRequest.Create("http://www.imdb.com/title/" + ImdbId + "/");
                    downloader.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
                    downloader.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
                    downloader.Headers[HttpRequestHeader.Pragma] = "no-cache";
                    downloader.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
                    downloader.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
                    var document = new HtmlDocument();
                    using (StreamReader sr = new StreamReader(downloader.GetResponse().GetResponseStream()))
                    {
                        document.Load(sr);
                    }
                    var nameNode = document.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    if (nameNode != null)
                    {
                        Name = nameNode.FirstChild.InnerText.Trim('\n', ' ');
                    }
                    else
                    {
                        nameNode = document.DocumentNode.SelectSingleNode("//h1[@class='header']");
                        Name = System.Net.WebUtility.HtmlDecode(nameNode.SelectSingleNode("span").InnerText.Trim());
                    }
                    var yearNode = document.DocumentNode.SelectSingleNode("//h1[@class='header']");
                    if (yearNode == null)
                        yearNode = document.DocumentNode.SelectSingleNode("//h1[@itemprop='name']");
                    if (yearNode != null)
                        yearNode = yearNode.Elements("span").FirstOrDefault(e => e.Element("a") != null);
                    Year = int.Parse(yearNode.Element("a").InnerText);
                    var ratingNode = document.DocumentNode.SelectSingleNode("//span[@itemprop='ratingValue']");
                    if (ratingNode != null && ratingNode.InnerText.Trim() != "-")
                    {
                        Rating = double.Parse(ratingNode.InnerText.Trim());
                        var ratingCount = document.DocumentNode.SelectSingleNode("//span[@itemprop='ratingCount']");
                        if (ratingCount != null)
                            RatingCount = int.Parse(ratingCount.InnerText.Trim(), System.Globalization.NumberStyles.AllowThousands);
                        var genresNode = document.DocumentNode.SelectSingleNode("//div[@class='infobar']");
                        if (genresNode != null)
                            Genres = genresNode.Elements("a").Select(n => System.Net.WebUtility.HtmlDecode(n.InnerText).Trim());
                        else
                            Genres = document.DocumentNode.SelectNodes("//span[@itemprop='genre']").Select(n => System.Net.WebUtility.HtmlDecode(n.InnerText).Trim());
                    }
                    break;
                }
                catch (Exception ex)
                { 
                    MessageBox.Show(ex.ToString());
                }
            }
        }
    }
}
