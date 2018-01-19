using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Net;
using HtmlAgilityPack;
using System.Threading;
using PriceCrawler.Models;

namespace PriceCrawler.CrawlProcess
{
    public class ShopeeProcessor : ICrawlProcessor
    {
        public void SearchKeyword(string keyword, string compareAccount, IWebDriver driver, WebDriverWait wait)
        {
            // 蝦皮查詢網址 https://shopee.tw/search/?keyword={keyword}&order=asc&page={page}&sortBy=price
            int page = 0;
            string url = $"https://shopee.tw/search/?keyword={ WebUtility.UrlEncode(keyword) }&order=asc&page={ page }&sortBy=price";
            driver.Navigate().GoToUrl(url);

            try
            {
                wait.Until(x => x.FindElement(By.ClassName("search-page__search-result")));
                wait.Until(x => x.FindElement(By.ClassName("shopee-page-controller")));
            }
            catch
            {
                return;
            }

            HtmlDocument doc = new HtmlDocument();
            doc.LoadHtml(driver.PageSource);
            int maxPage = GetMaxPage(doc);

            while (page < maxPage)
            {
                HtmlNodeCollection nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'shopee-search-result-view__item-card')]");
                int errCount = 0;

                while (nodes == null)
                {
                    Thread.Sleep(1000);
                    doc.LoadHtml(driver.PageSource);
                    nodes = doc.DocumentNode.SelectNodes("//*[contains(@class, 'shopee-search-result-view__item-card')]");
                    ++errCount;
                    //System.Windows.MessageBox.Show(errCount.ToString());

                    if (errCount > 10)
                    {
                        return;
                    }
                }

                foreach (HtmlNode node in nodes)
                {
                    string href = node.SelectSingleNode("a").GetAttributeValue("href", "");
                    string productUrl = $"https://shopee.tw{ href }";

                    driver.Navigate().GoToUrl(productUrl);
                    try
                    {
                        wait.Until(x => x.FindElement(By.ClassName("shopee-product-detail__parameters__content")));
                    }
                    catch
                    {
                        continue;
                    }

                    HtmlDocument productDoc = new HtmlDocument();
                    productDoc.LoadHtml(driver.PageSource);

                    HtmlNode productName = productDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'shopee-product-info__header__text')]");
                    HtmlNode productPrice = productDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'shopee-product-info__header__real-price')]");
                    HtmlNode account = productDoc.DocumentNode.SelectSingleNode("//*[contains(@class, 'product-page-seller-info__shop-name')]");

                    if (String.Equals(account.InnerText, compareAccount))
                    {
                        return;
                    }

                    // Write ExcelData
                    // 商品名稱 商品連結 商品價格 賣場連結 搜尋關鍵字
                    // productName, productUrl , price, $"https://shopee.tw/{ account }", keyword
                    Config.ExcelData.Add(new ExcelData
                    {
                        ProductName = productName.InnerText,
                        ProductUrl = productUrl,
                        ProductPrice = productPrice.InnerText,
                        AccountUrl = $"https://shopee.tw/{ account.InnerText }",
                        Keyword = keyword
                    });
                }

                ++page;
                url = $"https://shopee.tw/search/?keyword={ WebUtility.UrlEncode(keyword) }&order=asc&page={ page }&sortBy=price";
                driver.Navigate().GoToUrl(url);
                try
                {
                    wait.Until(x => x.FindElement(By.ClassName("search-page__search-result")));
                    wait.Until(x => x.FindElement(By.ClassName("shopee-page-controller")));
                }
                catch
                {
                    continue;
                }
            }
        }

        /// <summary>
        /// 取得最大查詢頁數 (xpath = "//*[contains(@class,'shopee-page-controller')]")
        /// </summary>
        /// <param name="doc">Html SourceCode</param>
        /// <returns></returns>
        private int GetMaxPage(HtmlDocument doc)
        {
            HtmlNode pageController = doc.DocumentNode.SelectSingleNode(@"//*[contains(@class,'shopee-page-controller')]");
            int maxPage = pageController.SelectNodes("button").Count - 2;

            return maxPage;
        }
    }
}
