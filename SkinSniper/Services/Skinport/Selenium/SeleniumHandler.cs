using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.Diagnostics;

namespace SkinSniper.Services.Skinport.Selenium
{
    internal class SeleniumHandler
    {
        private readonly string _baseUrl;
        private readonly string _username;
        private readonly string _password;

        private readonly IWebDriver _driver;
        private readonly IJavaScriptExecutor _javascript;
        public event EventHandler<string>? LoggedIn;

        public SeleniumHandler(string baseUrl, string userAgent, string username, string password)
        {
            _baseUrl = baseUrl;
            _username = username;
            _password = password;

            var service = ChromeDriverService.CreateDefaultService();
            service.SuppressInitialDiagnosticInformation = true;
            service.EnableVerboseLogging = false;
            service.HideCommandPromptWindow = true;

            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--headless");
            options.AddArgument("--window-size=1280,1080");
            options.AddArgument($"user-agent={userAgent}");

            // setup chrome driver
            _driver = new ChromeDriver(service, options);
            _javascript = (IJavaScriptExecutor)_driver;
        }

        public void Start()
        {
            // navigate to skinport
            _driver.Navigate().GoToUrl($"{_baseUrl}signin");

            // fill login details
            _driver.FindElement(By.Id("email")).SendKeys(_username);
            _driver.FindElement(By.Id("password")).SendKeys(_password);

            // log-in
            _driver.FindElement(By.ClassName("SubmitButton")).Click();

            new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(driver => driver.Url == $"{_baseUrl}account/confirm-new-device" || driver.Url == _baseUrl);
            Trace.WriteLine("Confirm New Device: ");

            _driver.Navigate().GoToUrl(Console.ReadLine());
            new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(driver => driver.Url == _baseUrl);

            // grab cookies
            var cookie = "";
            foreach (var entry in _driver.Manage().Cookies.AllCookies)
            {
                cookie += $"{entry.Name}={entry.Value}; ";
            }

            // call event
            _driver.Navigate().GoToUrl($"{_baseUrl}cart");
            LoggedIn?.Invoke(this, cookie);
        }

        public void Stop()
        {
            _driver.Quit();
        }

        public Task<string> GenerateTokenAsync()
        {
            return Task.Run(() =>
            {
                return GenerateToken();
            });
        }

        public string GenerateToken()
        {
            // execute grecaptcha to generate a token
            var watch = Stopwatch.StartNew();
            object data = _javascript.ExecuteScript("return await grecaptcha.enterprise.execute('6Ldo-yEgAAAAAIBUo13yCs0Pjek0XuIKUIS6lHFJ', {action: 'checkout'});");
            watch.Stop();

            Trace.WriteLine($"(Captcha): {watch.ElapsedMilliseconds}ms");
            return (string)data;
        }
    }
}
