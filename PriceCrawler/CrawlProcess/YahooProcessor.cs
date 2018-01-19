using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using HtmlAgilityPack;
using System.Text.RegularExpressions;
using PriceCrawler.Models;

namespace PriceCrawler.CrawlProcess
{
    public class YahooProcessor : ICrawlProcessor
    {
        public void SearchKeyword(string keyword, string compareAccount, IWebDriver driver, WebDriverWait wait)
        {
            // Yahoo 查詢網址 https://tw.bid.yahoo.com/search/auction/product?disp=list&kw={ keyword }&p={ keyword }&pg={ page }&sort=curp
            int page = 1;
            string url = $"https://tw.bid.yahoo.com/search/auction/product?disp=list&kw={ keyword }&p={ keyword }&pg={ page }&sort=curp";

            int errCount = 0;
            while (true)
            {
                driver.Navigate().GoToUrl(url);
                try
                {
                    wait.Until(d => d.FindElement(By.ClassName("Pagination__numberContainer___2oWVw")));
                    break;
                }
                catch
                {
                    ++errCount;
                    if (errCount > 10)
                    {
                        return;
                    }
                    continue;
                }
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);
            int maxPage = GetMaxPage(doc);

            while (page <= maxPage)
            {
                //ListItem__listitem___Ik9jn
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'ListItem__listitem___Ik9jn')]");

                foreach (HtmlNode node in nodes)
                {
                    // itemImageTag bottomRight 注目商品
                    HtmlNode exposure = node.SelectSingleNode("a[contains(@class, 'ListItem__imgs___3wJTS')]").SelectSingleNode("div[contains(@class, 'SquareImage__img___RoJr-')]").SelectSingleNode("span[contains(@class, 'itemImageTag bottomRight')]");
                    if (exposure == null)
                    {
                        string productName = node.SelectSingleNode("div[contains(@class, 'ListItem__detail___EEPbr')]").SelectSingleNode("a[contains(@class, 'ListItem__title___3CH7e')]").InnerText;
                        string productUrl = node.SelectSingleNode("div[contains(@class, 'ListItem__detail___EEPbr')]").SelectSingleNode("a[contains(@class, 'ListItem__title___3CH7e')]").GetAttributeValue("href", "");
                        string productPrice = node.SelectSingleNode("div[contains(@class, 'ListItem__detail___EEPbr')]").SelectSingleNode("div[contains(@class, 'ListItem__price___2CMKZ')]").SelectSingleNode("span[contains(@class, 'ListItem__red___3ja03')]").InnerText;
                        string accountUrl = node.SelectSingleNode("div[contains(@class, 'ListItem__shop___Z_sCW')]").SelectSingleNode("a").GetAttributeValue("href", "");

                        Regex rgx = new Regex(@"user\/(\w*)");
                        Match m = rgx.Match(accountUrl);
                        string account = m.Groups[1].Value;

                        if (String.Equals(account, compareAccount))
                        {
                            return;
                        }

                        Config.ExcelData.Add(new ExcelData
                        {
                            ProductName = productName,
                            ProductUrl = productUrl,
                            ProductPrice = productPrice,
                            AccountUrl = accountUrl,
                            Keyword = keyword
                        });
                    }
                    else
                    {
                        // 跳過注目商品
                        continue;
                    }
                }

                ++page;
                url = $"https://tw.bid.yahoo.com/search/auction/product?disp=list&kw={ keyword }&p={ keyword }&pg={ page }&sort=curp";
                while (true)
                {
                    driver.Navigate().GoToUrl(url);
                    try
                    {
                        wait.Until(d => d.FindElement(By.ClassName("Pagination__numberContainer___2oWVw")));
                        break;
                    }
                    catch
                    {
                        ++errCount;
                        if (errCount > 10)
                        {
                            return;
                        }
                        continue;
                    }
                }
            }
        }

        /// <summary>
        /// 取得最大查詢頁數 (xpath = "//*[contains(@class, 'Pagination__numberContainer___2oWVw')]")
        /// </summary>
        /// <param name="doc">Html SourceCode</param>
        /// <returns></returns>
        private int GetMaxPage(HtmlDocument doc)
        {
            HtmlNode pageController = doc.DocumentNode.SelectSingleNode("//*[contains(@class, 'Pagination__numberContainer___2oWVw')]");
            int maxPage = pageController.SelectNodes("a").Count;

            return maxPage;
        }
    }
}
