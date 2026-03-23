namespace MyNewsFeed.Services

{
    using MyNewsFeed.Models;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Xml.Linq;

    public class RssService
    {
        private readonly HttpClient _http;

        public RssService(HttpClient http)
        {
            _http = http;
        }

        public async Task<List<Article>> GetArticlesAsync(string feedUrl, string sourceName, string category)
        {
            var articles = new List<Article>();

            string xmlString;

            try
            {
                // 1. Download the raw XML from the RSS feed
                var response = await _http.GetAsync(feedUrl);
                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[RssService] Skipping feed ({(int)response.StatusCode}): {feedUrl}");
                    return articles;
                }

                xmlString = await response.Content.ReadAsStringAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RssService] Failed to fetch feed: {feedUrl} — {ex.Message}");
                return articles;
            }

            try
            {
                // 2. Read the XML
                var xml = XDocument.Parse(xmlString);

                // 3. Loop through every <item> in the feed and turn it into an Article
                foreach (var item in xml.Descendants("item"))
                {
                    var rawDesc = item.Element("description")?.Value ?? "";

                    // Call our "Separator"
                    var (cleanDescription, extractedUrl) = SplitImageAndDescription(rawDesc);

                    // Some feeds have the image in a separate tag; we check that first, 
                    // then fallback to the one we just "scraped" from the description.
                    var finalImageUrl =
                        item.Elements().FirstOrDefault(x => x.Name.LocalName == "thumbnail" || x.Name.LocalName == "content")
                        ?.Attribute("url")?.Value

                        // Check <enclosure url="..." type="image/..."> 
                        ?? item.Elements().FirstOrDefault(x =>
                            x.Name.LocalName == "enclosure" &&
                            (x.Attribute("type")?.Value.StartsWith("image/") ?? false))
                        ?.Attribute("url")?.Value

                        // Fallback to the URL we "scraped" from the FT description
                        ?? extractedUrl

                        // Final safety: empty string so the app doesn't crash
                        ?? "";

                    // Get Description
                    var finalDescription =
                        (!string.IsNullOrWhiteSpace(cleanDescription) ? cleanDescription : null)
                        ?? item.Element("description")?.Value ?? "";

                    articles.Add(new Article
                    {
                        Headline = item.Element("title")?.Value ?? "No Title",
                        Description = finalDescription,
                        Link = item.Element("link")?.Value ?? "#",
                        // RSS feeds usually store images in an <enclosure> tag
                        ImageUrl = finalImageUrl,
                        Source = sourceName,
                        PublishedDate = DateTime.TryParse(item.Element("pubDate")?.Value, out var dt) ? dt : DateTime.Now,
                        Category = category
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RssService] Failed to parse feed: {feedUrl} — {ex.Message}");
            }

            return articles;
        }

        private (string cleanDescription, string ImageUrl) SplitImageAndDescription(string rawHtml)
        {
            if (string.IsNullOrWhiteSpace(rawHtml)) return ("", "");

            // 1. Snatch the Image URL
            var imgMatch = Regex.Match(rawHtml, "<img.+?src=[\"'](.+?)[\"'].*?>", RegexOptions.IgnoreCase);
            string imageUrl = imgMatch.Success ? imgMatch.Groups[1].Value : "";

            // 2. NEW: Remove <a> tags AND their content (removes "World", "US", etc.)
            // This targets everything from <a... to </a>
            string textWithoutLinks = Regex.Replace(rawHtml, @"<a\b[^>]*>.*?</a>", string.Empty, RegexOptions.IgnoreCase);

            // 3. Strip remaining tags (like the <br /> and the <img> tag itself)
            string noTags = Regex.Replace(textWithoutLinks, "<.*?>", string.Empty);

            // 4. Decode and Trim (Fixes quotes, apostrophes, and extra spaces)
            string cleanDescription = WebUtility.HtmlDecode(noTags).Trim();

            return (cleanDescription, imageUrl);
        }

    }
}