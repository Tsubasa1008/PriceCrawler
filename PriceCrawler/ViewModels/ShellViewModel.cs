using Caliburn.Micro;
using ClosedXML.Excel;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using PriceCrawler.CrawlProcess;
using PriceCrawler.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PriceCrawler.ViewModels
{
    public class ShellViewModel : Conductor<object>
    {
        public ShellViewModel()
        {
            SelectedWebsite = Websites.First();
            LoadKeywords();
        }

        #region -- Properties --

        private string _keyword = "";
        private string _selectedKeyword;
        private BindableCollection<string> _keywords = new BindableCollection<string>();
        private Website _selectedWebsite;
        // setting Website info
        private BindableCollection<Website> _websites = new BindableCollection<Website>(
            new List<Website>()
            {
                new Website(){ Type = WebsiteType.Shopee, DisplayName = "蝦皮拍賣", Url = @"https://shopee.tw/", CompareAccount = "shinyinglee" },
                new Website(){ Type = WebsiteType.Yahoo, DisplayName = "奇摩拍賣", Url = @"https://tw.bid.yahoo.com/", CompareAccount = "Y9723212796" }
            }
        );
        public ICrawlProcessor Processor { get; set; }
        public string SaveFilePath { get; set; }

        public BindableCollection<Website> Websites
        {
            get { return _websites; }
            set
            {
                _websites = value;
                NotifyOfPropertyChange(() => Websites);
            }
        }

        public Website SelectedWebsite
        {
            get { return _selectedWebsite; }
            set
            {
                _selectedWebsite = value;
                NotifyOfPropertyChange(() => SelectedWebsite);
            }
        }

        public string Keyword
        {
            get { return _keyword; }
            set
            {
                _keyword = value;
                NotifyOfPropertyChange(() => Keyword);
                NotifyOfPropertyChange(() => CanAddKeyword);
            }
        }

        public BindableCollection<string> Keywords
        {
            get { return _keywords; }
            set
            {
                _keywords = value;
                NotifyOfPropertyChange(() => Keywords);
                NotifyOfPropertyChange(() => CanStartCrawl);
            }
        }

        public string SelectedKeyword
        {
            get { return _selectedKeyword; }
            set
            {
                _selectedKeyword = value;
                NotifyOfPropertyChange(() => SelectedKeyword);
                NotifyOfPropertyChange(() => CanRemoveKeyword);
            }
        }

        #endregion

        #region -- Methods --

        public bool CanAddKeyword
        {
            get
            {
                return !String.IsNullOrEmpty(Keyword);
            }
        }

        public void AddKeyword()
        {
            Keywords.Add(Keyword);
            NotifyOfPropertyChange(() => CanStartCrawl);
            Keyword = "";
        }

        public bool CanRemoveKeyword
        {
            get
            {
                if (SelectedKeyword != null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void RemoveKeyword()
        {
            Keywords.Remove(SelectedKeyword);
            NotifyOfPropertyChange(() => CanStartCrawl);
        }

        public bool CanStartCrawl
        {
            get
            {
                if (Keywords.Count > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public void StartCrawl()
        {
            //System.Windows.MessageBox.Show(Directory.GetCurrentDirectory());
            ChromeOptions options = new ChromeOptions();
            options.AddArgument("--disable-infobars");
            IWebDriver driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5.00));

            switch (SelectedWebsite.Type)
            {
                case WebsiteType.Yahoo:
                    Processor = new YahooProcessor();
                    SaveFilePath = Config.YahooFileSavePath;
                    break;
                case WebsiteType.Shopee:
                    Processor = new ShopeeProcessor();
                    SaveFilePath = Config.ShopeeFileSavePath;
                    break;
                default:
                    break;
            }

            // 清空資料
            Config.ExcelData = new List<ExcelData>();

            foreach (string keyword in Keywords)
            {
                Processor.SearchKeyword(keyword, SelectedWebsite.CompareAccount, driver, wait);
            }

            // 寫入 Excel
            WriteExcelFile();

            driver.Quit();
        }

        public void Cancel()
        {
            TryClose();
        }

        private void LoadKeywords()
        {
            if (File.Exists(Config.KeywordFilePath))
            {
                Keywords = new BindableCollection<string>(File.ReadAllLines(Config.KeywordFilePath).ToList());
            }
            else
            {
                Keywords = new BindableCollection<string>();
            }
        }

        private void WriteKeywords()
        {
            FileInfo file = new FileInfo(Config.KeywordFilePath);
            file.Directory.Create();
            File.WriteAllLines(Config.KeywordFilePath, Keywords);
        }

        public void OnClose()
        {
            WriteKeywords();
        }

        public void WriteExcelFile()
        {
            if (Config.ExcelData.Count > 0)
            {
                if (File.Exists(SaveFilePath))
                {
                    File.Delete(SaveFilePath);
                }
                FileInfo fp = new FileInfo(SaveFilePath);
                fp.Directory.Create();

                XLWorkbook wb = new XLWorkbook();
                var ws = wb.AddWorksheet("Sheet1");
                // 商品名稱 商品連結 商品價格 賣場連結 搜尋關鍵字
                ws.Row(1).Cell(1).SetValue("商品名稱");
                ws.Row(1).Cell(2).SetValue("商品連結");
                ws.Row(1).Cell(3).SetValue("商品價格");
                ws.Row(1).Cell(4).SetValue("賣場連結");
                ws.Row(1).Cell(5).SetValue("搜尋關鍵字");


                foreach (ExcelData data in Config.ExcelData)
                {
                    int currRow = ws.LastRowUsed().RowNumber() + 1;

                    ws.Row(currRow).Cell(1).SetValue(data.ProductName);
                    ws.Row(currRow).Cell(2).SetValue(data.ProductUrl);
                    ws.Row(currRow).Cell(3).SetValue(data.ProductPrice);
                    ws.Row(currRow).Cell(4).SetValue(data.AccountUrl);
                    ws.Row(currRow).Cell(5).SetValue(data.Keyword);
                }

                ws.SheetView.FreezeRows(1);
                ws.Columns().AdjustToContents();
                wb.SaveAs(SaveFilePath);
            }
        }

        #endregion
    }
}
