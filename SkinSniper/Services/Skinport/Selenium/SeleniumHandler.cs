using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using SeleniumUndetectedChromeDriver;
using SkinSniper.Config;
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
            //options.AddArgument("--headless");
            options.AddArgument("--window-size=1280,1080");
            options.AddArgument($"--user-agent={userAgent}");

            // setup chrome driver
            var installer = new ChromeDriverInstaller().Auto();
            installer.Wait();

            _driver = UndetectedChromeDriver.Create(options, driverExecutablePath: installer.Result);
            _javascript = (IJavaScriptExecutor)_driver;
        }

        public void Start()
        {
            // navigate to skinport
            _driver.Navigate().GoToUrl(_baseUrl);

            // attempt to log-in using saved cookie
            if (ConfigHandler.Get().Skinport.Cookie != null)
            {
                // set the cookie into the session
                var cookies = ConfigHandler.Get().Skinport.Cookie.TrimEnd(';').Split(';');
                if (cookies.Length > 1)
                {
                    foreach (var split in cookies.Select(c => c.Split('=')))
                    {
                        _driver.Manage().Cookies.AddCookie(new Cookie(split[0], split[1]));
                    }

                    // set localstorage, to force skinport to load new data
                    /*
                        localStorage.setItem("user", JSON.stringify({}));
                        localStorage.setItem("session", JSON.stringify({ allCookiesAccepted: true }));
                        arguments[0]();
                    */
                    _javascript.ExecuteAsyncScript("localStorage.setItem(\"user\",JSON.stringify({})),localStorage.setItem(\"session\",JSON.stringify({allCookiesAccepted:!0})),arguments[0]();");

                    // reload the page
                    _driver.Navigate().GoToUrl($"{_baseUrl}account");

                    // wait to redirect to new page
                    try
                    {
                        new WebDriverWait(_driver, TimeSpan.FromSeconds(10)).Until(driver => driver.Url == $"{_baseUrl}account");
                    }
                    catch (WebDriverTimeoutException)
                    {
                        Console.WriteLine("Failed to log-in using cookie, trying manual log-in!");
                    }
                }

                // logged in woo!
                if (_driver.Url == $"{_baseUrl}account")
                {
                    Console.WriteLine("Successfuly logged in using cookie!");
                }
                else
                {
                    // wait for page to load
                    _driver.Navigate().GoToUrl($"{_baseUrl}signin");

                    // fill login details
                    _driver.FindElement(By.Id("email"), 30).SendKeys(_username);
                    _driver.FindElement(By.Id("password")).SendKeys(_password);

                    // wait for submit to be clickable
                    var submit = _driver.FindElement(By.ClassName("SubmitButton"));
                    new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(driver => submit.GetAttribute("disabled") == null);

                    // log-in
                    submit.Click();

                    new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(driver => driver.Url == $"{_baseUrl}account/confirm-new-device" || driver.Url == _baseUrl);
                    Trace.WriteLine("Confirm New Device: ");

                    _driver.Navigate().GoToUrl(Console.ReadLine());
                    new WebDriverWait(_driver, TimeSpan.FromSeconds(30)).Until(driver => driver.Url == _baseUrl);

                    // grab cookies
                    var cookie = "";
                    foreach (var entry in _driver.Manage().Cookies.AllCookies)
                    {
                        cookie += $"{entry.Name}={entry.Value};";
                    }

                    // save cookie into config
                    ConfigHandler.Get().Skinport.Cookie = cookie;
                    ConfigHandler.Save();
                }

                // setup captcha and invoke event
                _driver.Navigate().GoToUrl($"{_baseUrl}support/new");
                _driver.FindElement(By.Id("cf-turnstile"), 30);

                // insert captcha script
                /*
                    const element = document.getElementById("cf-turnstile");
                    turnstile.remove(element);
                    turnstile.render(element, {
                      action: "checkout",
                      execution: "execute",
                      sitekey: "0x4AAAAAAADTS9QyreZcUSn1",
                      callback: e => window.callback(e)
                    });
                */
                _javascript.ExecuteScript("const element=document.getElementById(\"cf-turnstile\");turnstile.remove(element),turnstile.render(element,{action:\"checkout\",execution:\"execute\",sitekey:\"0x4AAAAAAADTS9QyreZcUSn1\",callback:e=>window.callback(e)});");

                // invoke logged in event
                LoggedIn?.Invoke(this, ConfigHandler.Get().Skinport.Cookie);
            }
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
            // simulate user action
            // - cloudflare prevents token generation after inactivity.
            _driver.FindElement(By.ClassName("TextField-input"), 10).Click();

            // execute cloudflare turnstile to generate a token
            /*
                const element = document.getElementById("cf-turnstile");
                window.callback = arguments[0];
                turnstile.execute(element); 
            */
            var watch = Stopwatch.StartNew();
            object data = _javascript.ExecuteAsyncScript("const element=document.getElementById(\"cf-turnstile\");window.callback=arguments[0],turnstile.execute(element);");
            watch.Stop();

            Trace.WriteLine($"(Captcha): {watch.ElapsedMilliseconds}ms");
            return (string)data;
        }
    }
}
