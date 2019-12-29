using System;
using System.Collections.Generic;
using System.Text;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;

namespace ReviewRetriever
{
    class WebScrapeUtil
    {
        private Driver driver { get; set; }

        public WebScrapeUtil(Driver dr) 
        {
            driver = dr;
        }

        public void GoTo(string url = "https://www.google.com") 
        {
            driver.Instance.Navigate().GoToUrl(url);
        }

        public IWebElement GetElement(string xPath) 
        {
            try
            {
                return driver.Instance.FindElement(By.XPath(xPath));
            }
            catch (Exception e)
            {
                return null; 
            }
        }
        public IWebElement[] GetElementsByTag(string sTag)
        {
            try
            {
                var wResults =  driver.Instance.FindElements(By.TagName(sTag));
                IWebElement[] wRetVals = new IWebElement[wResults.Count];
                wResults.CopyTo(wRetVals, 0);
                return wRetVals;
            }
            catch (Exception e)
            {
                return null;
            }
        }

        public string GetCurPageStr() 
        {
            return driver.Instance.PageSource.ToString();
        }

        public string getHtmlStr(string url)
        {
            GoTo(url);
            return GetCurPageStr();
        }

    }
}
