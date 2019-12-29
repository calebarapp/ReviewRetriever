using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System;

namespace ReviewRetriever
{
    public class Driver : IDisposable
    {
        public IWebDriver Instance { get; internal set; }

        public Driver()
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--headless --disable-gpu");
            Instance = new ChromeDriver(chromeOptions);
            WaitOn();
        }

        public void Close()
        {
            Instance.Close();
        }

        public WebDriverWait WebDriverWait
        {
            get
            {
                return new WebDriverWait(Instance, TimeSpan.FromSeconds(20));
            }
        }

        public dynamic NoWait(Func<bool> action)
        {
            WaitOff();
            var result = action();
            WaitOn();
            return result;
        }

        private void WaitOn()
        {
            Instance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
        }

        private void WaitOff()
        {
            Instance.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(0);

        }

        public void Dispose()
        {
            Close();
        }
    }
}
