using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace PriceCrawler.CrawlProcess
{
    public interface ICrawlProcessor
    {
        void SearchKeyword(string keyword, string compareAccount, IWebDriver driver, WebDriverWait wait);
    }
}
