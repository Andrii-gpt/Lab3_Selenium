using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;

namespace Lab3_Selenium
{
    public class Variant2Tests
    {
        private IWebDriver driver = null!;
        private string downloadDir = null!;
        private const string BaseUrl = "https://the-internet.herokuapp.com";

        [SetUp]
        public void Setup()
        {
            downloadDir = Path.Combine(TestContext.CurrentContext.WorkDirectory, "Downloads");
            Directory.CreateDirectory(downloadDir);

            var options = new ChromeOptions();
            options.AddUserProfilePreference("download.default_directory", downloadDir);
            options.AddUserProfilePreference("download.prompt_for_download", false);

            driver = new ChromeDriver(options);
            driver.Manage().Window.Maximize();
        }

        [Test]
        public void ForgotPassword_SubmitEmail_ShowsEmailSentMessage()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/forgot_password");

            Assert.That(driver.FindElement(By.TagName("h2")).Text, Is.EqualTo("Forgot Password"));

            var email = driver.FindElement(By.Id("email"));
            var button = driver.FindElement(By.Id("form_submit"));

            Assert.That(email.Displayed, Is.True);
            Assert.That(button.Enabled, Is.True);
            Assert.That(button.Text, Is.EqualTo("Retrieve password"));

            email.SendKeys("tkach.test@example.com");
            Assert.That(email.GetDomProperty("value"), Is.EqualTo("tkach.test@example.com"));

            button.Click();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => d.Url.EndsWith("/email_sent"));

            Assert.That(driver.FindElement(By.Id("content")).Text, Does.Contain("Your e-mail's been sent!"));
        }

        [Test]
        public void EntryAd_CloseModal_ModalNotShownAfterReload()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/entry_ad");

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var modal = driver.FindElement(By.Id("modal"));
            wait.Until(_ => modal.Displayed);

            Assert.That(driver.FindElement(By.CssSelector("#modal .modal-title")).Text,
                Is.EqualTo("This is a modal window").IgnoreCase);

            driver.FindElement(By.CssSelector("#modal .modal-footer p")).Click();
            wait.Until(_ => !modal.Displayed);
            Thread.Sleep(1000);

            driver.Navigate().Refresh();
            Thread.Sleep(2000);

            Assert.That(driver.FindElement(By.Id("modal")).Displayed, Is.False);
        }

        [Test]
        public void FileDownload_ClickLink_FileIsSaved()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/download");

            Assert.That(driver.FindElement(By.TagName("h3")).Text, Is.EqualTo("File Downloader"));

            var links = driver.FindElements(By.CssSelector("#content a[href*='download/']"));
            Assert.That(links, Is.Not.Empty);

            var link = links.FirstOrDefault(l => l.Text.EndsWith(".txt")) ?? links[0];
            var filePath = Path.Combine(downloadDir, link.Text);
            if (File.Exists(filePath))
                File.Delete(filePath);

            link.Click();

            new WebDriverWait(driver, TimeSpan.FromSeconds(15)).Until(_ => File.Exists(filePath));
            Assert.That(File.Exists(filePath), Is.True);
        }

        [Test]
        public void BasicAuth_ValidCredentials_ShowsSuccessMessage()
        {
            driver.Navigate().GoToUrl("https://admin:admin@the-internet.herokuapp.com/basic_auth");

            Assert.That(driver.FindElement(By.TagName("h3")).Text, Is.EqualTo("Basic Auth"));
            Assert.That(driver.FindElement(By.CssSelector("#content p")).Text,
                Is.EqualTo("Congratulations! You must have the proper credentials."));
        }
        [Test]
        public void DynamicLoading_ClickStart_ShowsHelloWorld()
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            driver.Navigate().GoToUrl(BaseUrl + "/dynamic_loading/1");

            var finish = driver.FindElement(By.Id("finish"));
            Assert.That(finish.Displayed, Is.False);

            driver.FindElement(By.CssSelector("#start button")).Click();
            wait.Until(_ => finish.Displayed);
            Assert.That(finish.Text, Is.EqualTo("Hello World!"));

            driver.Navigate().GoToUrl(BaseUrl + "/dynamic_loading/2");

            Assert.That(driver.FindElements(By.Id("finish")), Is.Empty);

            driver.FindElement(By.CssSelector("#start button")).Click();
            var rendered = wait.Until(d =>
            {
                var elements = d.FindElements(By.Id("finish"));
                return elements.Count > 0 && elements[0].Displayed ? elements[0] : null;
            });
            Assert.That(rendered!.Text, Is.EqualTo("Hello World!"));
        }

        [Test]
        public void ContextMenu_RightClick_ShowsAlert()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/context_menu");

            var hotSpot = driver.FindElement(By.Id("hot-spot"));
            Assert.That(hotSpot.Displayed, Is.True);

            new Actions(driver).ContextClick(hotSpot).Perform();

            var alert = new WebDriverWait(driver, TimeSpan.FromSeconds(5)).Until(d =>
            {
                try { return d.SwitchTo().Alert(); }
                catch (NoAlertPresentException) { return null; }
            });

            Assert.That(alert!.Text, Is.EqualTo("You selected a context menu"));
            alert.Accept();

            Assert.Throws<NoAlertPresentException>(() => driver.SwitchTo().Alert());
        }

        [Test]
        public void RedirectLink_Click_OpensStatusCodesPage()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/redirector");

            Assert.That(driver.FindElement(By.TagName("h3")).Text, Is.EqualTo("Redirection"));

            var link = driver.FindElement(By.Id("redirect"));
            Assert.That(link.Text, Is.EqualTo("here"));

            link.Click();
            new WebDriverWait(driver, TimeSpan.FromSeconds(10)).Until(d => d.Url.EndsWith("/status_codes"));

            Assert.That(driver.FindElement(By.TagName("h3")).Text, Is.EqualTo("Status Codes"));
            Assert.That(driver.FindElements(By.CssSelector("#content ul li a")).Count, Is.EqualTo(4));
        }
        [TearDown]
        public void TearDown()
        {
            driver.Quit();
            driver.Dispose();
        }
        [Test]
        public void HorizontalSlider_MoveWithKeyboard_UpdatesValue()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/horizontal_slider");

            var slider = driver.FindElement(By.CssSelector("input[type='range']"));
            var value = driver.FindElement(By.Id("range"));

            Assert.That(slider.Displayed, Is.True);
            Assert.That(slider.GetDomAttribute("max"), Is.EqualTo("5.0"));
            Assert.That(value.Text, Is.EqualTo("0"));

            slider.Click();
            slider.SendKeys(Keys.Home);
            Assert.That(value.Text, Is.EqualTo("0"));

            slider.SendKeys(Keys.ArrowRight + Keys.ArrowRight + Keys.ArrowRight);
            Assert.That(value.Text, Is.EqualTo("1.5"));
            Assert.That(slider.GetDomProperty("value"), Is.EqualTo("1.5"));

            slider.SendKeys(Keys.End);
            Assert.That(value.Text, Is.EqualTo("5"));
        }

        [Test]
        public void Dropdown_SelectOptions_ChangesSelectedOption()
        {
            driver.Navigate().GoToUrl(BaseUrl + "/dropdown");

            Assert.That(driver.FindElement(By.TagName("h3")).Text, Is.EqualTo("Dropdown List"));

            var select = new SelectElement(driver.FindElement(By.Id("dropdown")));

            Assert.That(select.Options.Count, Is.EqualTo(3));
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Please select an option"));
            Assert.That(select.Options[0].Enabled, Is.False);

            select.SelectByText("Option 1");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 1"));

            select.SelectByValue("2");
            Assert.That(select.SelectedOption.Text, Is.EqualTo("Option 2"));
            Assert.That(select.SelectedOption.GetDomAttribute("selected"), Is.Not.Null);
        }

        [Test]
        public void Typos_ReloadPage_DetectsRandomTypo()
        {
            const string intro = "This example demonstrates a typo being introduced. It does it randomly on each page load.";
            const string correct = "Sometimes you'll see a typo, other times you won't.";
            const string typo = "Sometimes you'll see a typo, other times you won,t.";

            var texts = new List<string>();

            for (int i = 0; i < 10; i++)
            {
                driver.Navigate().GoToUrl(BaseUrl + "/typos");

                var paragraphs = driver.FindElements(By.CssSelector("#content p"));
                Assert.That(paragraphs.Count, Is.EqualTo(2));
                Assert.That(paragraphs[0].Text, Is.EqualTo(intro));

                texts.Add(paragraphs[1].Text);
            }

            Assert.That(texts.All(t => t == correct || t == typo), Is.True);
            Assert.That(texts, Does.Contain(typo));
        }
    }
}