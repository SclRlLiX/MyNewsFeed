namespace MyNewsFeed.Services

{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Xml.Linq;
    using MyNewsFeed.Models;

    public class RssService
    {
        private readonly HttpClient _http;

        public RssService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Article>> GetArticlesAsync(string feedUrl)
        {
            var articles = new List<Article>();

            // 1. Download the raw XML from the RSS feed
            var xmlString = await _http.GetStringAsync(feedUrl);

            // 2. Read the XML
            var xml = XDocument.Parse(xmlString);

            // 3. Loop through every <item> in the feed and turn it into an Article
            foreach (var item in xml.Descendants("item"))
            {
                articles.Add(new Article
                {
                    Headline = item.Element("title")?.Value ?? "No Title",
                    Description = item.Element("description")?.Value ?? "",
                    Link = item.Element("link")?.Value ?? "#",
                    // RSS feeds usually store images in an <enclosure> tag
                    ImageUrl = item.Elements()
                       .FirstOrDefault(x => x.Name.LocalName == "thumbnail" || x.Name.LocalName == "content")
                       ?.Attribute("url")?.Value ?? ""
                        });
            }

            return articles;
        }
    }
}
