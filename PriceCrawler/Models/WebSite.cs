namespace PriceCrawler.Models
{
    public class Website
    {
        public WebsiteType Type { get; set; }

        public string DisplayName { get; set; }

        public string Url { get; set; }

        public string CompareAccount { get; set; }
    }

    public enum WebsiteType
    {
        Yahoo,
        Shopee
    }
}
