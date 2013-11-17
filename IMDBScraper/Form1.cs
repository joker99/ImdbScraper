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
            var imdbIds = txtIO.Text.Split(new string[] {Environment.NewLine}, StringSplitOptions.RemoveEmptyEntries);
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

        private void txtIO_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.A)
                txtIO.SelectAll();
        }
    }
}
