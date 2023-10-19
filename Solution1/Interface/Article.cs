namespace Interface
{
    public class Article
    {
        public string title;
        public string description;
        public string url;
        public string date;

        public Article(string title, string url, string description, string date)
        {
            this.title = title;
            this.url = url;
            this.description = description;
            this.date = date;
        }
    }
}
