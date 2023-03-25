using System;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace SkinSniper.Services.Skinport.Selenium
{
    public static class WebDriverExtensions
    {
        public static IWebElement FindElement(this IWebDriver driver, By by, int timeoutInSeconds)
        {
            if (timeoutInSeconds > 0)
            {
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutInSeconds));
                return wait.Until(drv =>
                {
                    IWebElement? element = null;

                    try
                    {
                        element = drv.FindElement(by);
                    }
                    catch
                    {
                        element = null;
                    }

                    return element;
                });
            }

            return driver.FindElement(by);
        }
    }
}

