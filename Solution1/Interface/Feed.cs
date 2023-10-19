using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Xml;

namespace Interface
{
    public class Feed
    {
        public string url;
        public string name;

        public double interval = 900000.0;

        private DateTime lastUpdated;

        public List<Article> articles;

        public Feed(string url, string customName)
        {
            articles = new();
            
            this.url = url;

            string feedName = Update();

            // Sets name to custom name if provided, or default name if not
            if (customName != "")
            {
                name = customName;
            }
            else
            {
                name = feedName;
            }
        }

        public string Update()
        {
            try
            {
                // Sets xml settings to make it read right
                XmlReaderSettings settings = new()
                {
                    DtdProcessing = DtdProcessing.Parse
                };

                // Creates an xml reader to get the data
                XmlReader reader = XmlReader.Create(url, settings);

                // Makes a feed with the data
                SyndicationFeed feed = SyndicationFeed.Load(reader);

                articles.Clear();

                // Gets the list of articles
                foreach (SyndicationItem item in feed.Items.ToList())
                {
                    Article article = new(item.Title.Text, item.Links.ToList()[0].Uri.ToString(), "", item.PublishDate.ToLocalTime().ToString());

                    articles.Add(article);
                }

                // Changes the last updated time to right now
                lastUpdated = DateTime.Now;

                return feed.Title.Text;
            }
            catch (Exception)
            {
                return "";
            }
        }

        public void CheckUpdate()
        {
            // Checks how long it's been
            TimeSpan span = DateTime.Now - lastUpdated;

            // Update if it's been longer than the interval
            if (span.TotalMilliseconds >= interval)
            {
                Update();
            }
        }
    }
}
