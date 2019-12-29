using System;
using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using System.Data;
using ReviewRetriever.Model;

namespace ReviewRetriever
{
    class YelpScrapeScript
    {
        private int MAX_ATTEMPTS = 20; //Max twenty attempts. Need to implement
        public DateTime dtFrom { get; set; }
        public string sStoreFilter { get; set; }

        public YelpScrapeScript() 
        {
            dtFrom = DateTime.MinValue;
        }

        public YelpScrapeScript(DateTime dtFrom)
        {
            this.dtFrom = dtFrom;
        }

        public YelpScrapeScript(string sStoreName)
        {
            sStoreFilter = sStoreName;
        }

        public YelpScrapeScript(DateTime dtFrom, string sStoreName)
        {
            this.dtFrom = dtFrom;
            sStoreFilter = sStoreName;
        }

        //=====================================================================================
        //WebScrape Script
        //=====================================================================================

        public void Execute() 
        {
            Logging.Log("YelpScrapeScript.Execute() -> Retriving all reviews.", LogLevel.Both);
            
            Driver driver           = new Driver();
            Store[] stores          = config.Stores;
            var reviews             = new List<Review>();
            
            foreach (Store store in stores) 
            {
                //Extract code
                DateTime dtFilter = (dtFrom == DateTime.MinValue) ? getLastDate(store.StoreName) : dtFrom;

                if (sStoreFilter != null)
                    if (store.StoreName.ToUpper() != sStoreFilter.ToUpper())
                        continue;
                if (dtFilter == DateTime.MinValue)
                    continue;
                Logging.Log("YelpScrapeScript.Execute() -> Retrieving reviews for store: " + store.StoreName + " Url: " + store.Url + "From: " + dtFilter.ToString(), LogLevel.Both);
                
                //Retrive Reviews
                try
                {
                    List<Review> reviewsFromDate = null;
                    int iAttempts = 0;
                    while (reviewsFromDate == null && MAX_ATTEMPTS >= iAttempts)
                    {
                        reviewsFromDate = ReviewsFromDate(store.StoreName, store.Url, driver, dtFilter);
                        if(reviewsFromDate != null)
                            reviews.AddRange(reviewsFromDate);
                        iAttempts++;
                    }
                }
                catch (Exception e) 
                {
                    Logging.Log("YelpScrapeScript.Execute() -> Error retrieving reviews for: " + store.StoreName + "! Error: " + e.Message, LogLevel.Both);
                }
            }
            Logging.Log("YelpScrapeScript.Execute() -> Retrieval complete.", LogLevel.Both);
            driver.Close();

            SaveReviews(reviews);
        }

        private DateTime getLastDate(string sStore) 
        {
            DateTime dtRet = DateTime.MinValue;

            string sql = string.Format("SELECT TOP(1) DATE FROM REVIEWS WHERE STORE = '{0}' order by CONVERT(DATE, date) desc", sStore);

            DbUtil db = new DbUtil();
            DataSet ds = new DataSet();
            if(db.GetSqlDataSet(sql, config.ConnectionString, ref ds) > 0) 
            {
                string sLastDt = ds.Tables[0].Rows[0]["DATE"].ToString();
                DateTime.TryParse(sLastDt, out dtRet);
            }
            return dtRet;
        }

        private string createInsertStatement(List<Review> reviews) 
        {

            string sSqlQuery = @"INSERT INTO dbo.REVIEWS ([DATE],[STORE],[RATING],[COMMENT],[ORDER]) VALUES ";

            Review review = null;
            string sLastDate = "";
            int iOrder = 0;
            for (int x = 0; x < reviews.Count(); x++)
            {
                review = reviews[x];
                string sDate = review.Date.ToString("MM/dd/yyyy");
                if (sLastDate == sDate)
                    iOrder++;
                else
                {
                    iOrder = 0;
                    sLastDate = sDate;
                }
                sSqlQuery += string.Format("('{0}','{1}','{2}','{3}','{4}')",
                        sDate, review.Store, review.Rating, review.Comment.Replace("'",""), iOrder
                );
                if (x + 1 != reviews.Count())
                    sSqlQuery += ",";
            }
            return sSqlQuery;
        }
         
        public bool SaveReviews(List<Review> reviews) 
        {
            bool bRetVal = false;
            string sSqlQuery = createInsertStatement(reviews);
            DbUtil db = new DbUtil();
            db.ExecuteSql(sSqlQuery, config.ConnectionString);
            return bRetVal;
        }
         
        //retrieves a list of reviews for a given store from a given date
        //The URL uses a parameter start=20 after the first page. Increments by 20.
        private List<Review> ReviewsFromDate(string store, string url, Driver driver, DateTime date) 
        {
            WebScrapeUtil scraper = new WebScrapeUtil(driver);
            List<Review> lsRetReviews = new List<Review>();        

            bool checkNextPage = true;
            int index = 0;

            while (checkNextPage)
            {
                string newUrl = urlAddReviewPage(index, url);
                scraper.GoTo(newUrl);

                 IWebElement[] wLists = null;
                try
                {
                    wLists = scraper.GetElementsByTag("ul")
                                                .Where(x => x.FindElements(By.TagName("li")).Count == 20)
                                                .ToArray();
                }
                catch { }
                if (wLists == null)
                    continue;
                IWebElement reviewBlock = wLists.Count() > 0 ? wLists[0] : null;
                if (reviewBlock == null)
                    break;

                List<Review> reviewsList = new List<Review>();
                RecordsFromReviewBlock(store, reviewBlock, ref reviewsList);
                try
                {
                    reviewsList = new List<Review>(reviewsList.Where(x => (x.Date.CompareTo(date)) > 0));
                    lsRetReviews.AddRange(reviewsList);
                }
                catch  {}


                if (reviewsList.Count < 17)
                {
                    checkNextPage = false;
                    break;
                }
                index++;
            }
            return lsRetReviews;
        }

        //Break the review block into induvidual reviews and extract data.
        private bool RecordsFromReviewBlock(string store, IWebElement reviewBlock, ref List<Review> reviewsList) 
        {
            bool bRetVal = true;
            var reviews = reviewBlock.FindElements(By.TagName("li"));
            foreach (var review in reviews)
            {
                var paragraphs = review.FindElements(By.TagName("p"));
                List<string> allParagraphText = new List<string>();

                //Get comment
                var spans = review.FindElements(By.TagName("span"));
                var divs = review.FindElements(By.TagName("div"));
                
                //Skip review if elements were not found
                if (spans == null || divs == null) 
                    continue;

                Review reviewRecord = new Review();
                reviewRecord.Comment = "";
                reviewRecord.Store = store;
                //get rating
                foreach (var div in divs)
                {
                    string className = div.GetAttribute("class");
                    if (className.Contains("star"))
                    {
                        string label = div.GetAttribute("aria-label");
                        reviewRecord.Rating = getInt(label);
                    }
                }
                //get date and comment
                foreach (var span in spans)
                {
                    string text = "";
                    try
                    {
                        string className = "";
                        try
                        {
                            text = span.Text;
                            className = span.GetAttribute("class");
                        }
                        catch { }
                        if (text != "")
                        {
                            if (reviewRecord.Date == DateTime.MinValue) 
                            {
                                DateTime dtTest = DateTime.Now;
                                if (DateTime.TryParse(text, out dtTest))
                                {
                                    reviewRecord.Date = dtTest;
                                }
                            }
                            else if (text.Length > reviewRecord.Comment.Length && text.Length > 20 && reviewRecord.Comment == "") 
                            {
                                    reviewRecord.Comment = text;
                            }
                        }
                    }
                    catch
                    {
                        return false;
                    }
                }
                if (reviewRecord.Date == null)
                    bRetVal = false;
                reviewsList.Add(reviewRecord);
            }
            return bRetVal;
        }

        private int getInt(string text) 
        {
            int iRetVal = -1;
            int.TryParse(new string(text.Where(x => char.IsDigit(x))
                            .ToArray()),
                            out iRetVal );
            return iRetVal;
        }

        private string urlAddReviewPage(int pageIndex, string url) 
        {
            if (pageIndex > 0)
            {
                string sUrlBody = url.Split("?")[0];
                string sFirstParam = url.Split("?")[1].Split("&")[0];
                string sUrlTail = url.Replace(sUrlBody + "?" + sFirstParam, "");
                url = sUrlBody + "?" + "start=" + (pageIndex * 20) + sUrlTail;
            }
            return url;
        }

    }
}
