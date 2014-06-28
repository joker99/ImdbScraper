using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Net;
using System.IO;

namespace IMDBScraper
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnScrape_Click(object sender, EventArgs e)
        {
            btnScrape.Enabled = false;
            var imdbIds = txtIO.Text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries).ToList();
            // the input can be a link to coming soon page (http://www.imdb.com/movies-coming-soon/) or list of imdb id's
            if (imdbIds.Count == 1 && imdbIds[0].IndexOf("imdb.com") != 0)
            {
                imdbIds = ParseComingSoonPage(imdbIds[0]).ToList();
            }
            // parse list of id's
            var result = string.Join(Environment.NewLine, imdbIds.AsParallel().Select((id, i) =>
            {
                var imdbEntry = new ImdbEntry(id);
                return new
                {
                    Key = i,
                    Value = String.Format("{0}\t{1:0.0}/10   {2:#,#} votes\t{3}\t{4}\t{5}", imdbEntry.Name, imdbEntry.Rating, imdbEntry.RatingCount, string.Join(" | ", imdbEntry.Genres), imdbEntry.ImdbId, imdbEntry.Year)
                };
            }).OrderBy(kvp => kvp.Key).Select(kvp=>kvp.Value));            
            txtIO.Text = result.ToString();
            btnScrape.Enabled = true;
        }

        private IEnumerable<string> ParseComingSoonPage(string url)
        {
            var downloader = (HttpWebRequest)HttpWebRequest.Create(url);
            downloader.Headers[HttpRequestHeader.AcceptLanguage] = "en-US,en;q=0.5";
            downloader.Headers[HttpRequestHeader.AcceptEncoding] = "gzip, deflate";
            downloader.Headers[HttpRequestHeader.Pragma] = "no-cache";
            downloader.UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
            downloader.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            var document = new HtmlAgilityPack.HtmlDocument();
            try
            {
                using (StreamReader sr = new StreamReader(downloader.GetResponse().GetResponseStream()))
                {
                    document.Load(sr);
                }
            }
            catch (WebException ex)
            {

            }

            var urlNodes = document.DocumentNode.SelectNodes("//a[@itemprop='url']");
            return urlNodes.Select(node=> node.Attributes["href"].Value.Split(new string[] {"/"},StringSplitOptions.RemoveEmptyEntries)[1]).Where(id=>id.StartsWith("tt"));
        }

        private void txtIO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                txtIO.SelectAll();
        }
    }
}
