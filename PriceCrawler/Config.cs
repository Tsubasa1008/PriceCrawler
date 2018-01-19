using PriceCrawler.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PriceCrawler
{
    public class Config
    {
        //public const string KeywordFilePath = @"D:\查價資料\Keywords.txt";

        public static List<ExcelData> ExcelData = new List<ExcelData>();

        public static string KeywordFilePath
        {
            get
            {
                return $@"{ Directory.GetCurrentDirectory() }\Keywords.txt";
            }
        }

        public static string ShopeeFileSavePath
        {
            get
            {
                return $@"{ Directory.GetCurrentDirectory() }\Result\Shopee_{ DateTime.Now.ToString("yyyyMMddHHmmss") }.xlsx";
            }
        }

        public static string YahooFileSavePath
        {
            get
            {
                return $@"{ Directory.GetCurrentDirectory() }\Result\Yahoo_{ DateTime.Now.ToString("yyyyMMddHHmmss") }.xlsx";
            }
        }
    }
}
