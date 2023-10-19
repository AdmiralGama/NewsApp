using System.Collections.Generic;
using System.Linq;

namespace Interface
{
    public class FeedManager
    {
        public List<Feed> feeds;

        public FeedManager()
        {
            feeds = new();
        }

        /// <summary>
        /// Creates an RSS Feed from a URL with a custom name
        /// </summary>
        /// <param name="url"></param>
        /// <param name="customName"></param>
        /// <returns></returns>
        public Feed CreateFeed(string url, string customName)
        {
            Feed feed = new(url, customName);
            feeds.Add(feed);
            return feed;
        }

        /// <summary>
        /// Gets a feed based on its name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Feed? GetFeed(string? name)
        {
            foreach (Feed feed in feeds)
            {
                if (feed.name == name)
                {
                    return feed;
                }
            }

            return null;
        }

        /// <summary>
        /// Makes articles check if they need to be updated
        /// </summary>
        public void CheckUpdate()
        {
            foreach (Feed feed in feeds)
            {
                feed.CheckUpdate();
            }
        }

        /// <summary>
        /// Gets a list of articles that match a certain topic keyword
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public List<Article> GetArticlesFromTopic(string topic)
        {
            List<Article> articles = new();

            // Creates the list of articles
            foreach (Feed feed in feeds)
            {
                foreach (Article article in feed.articles)
                {
                    // Checks if the keyword shows up in the title
                    if (article.title.Split().Contains(topic))
                    {
                        articles.Add(article);
                    }
                }
            }

            return articles;
        }
    }
}