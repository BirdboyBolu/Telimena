﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities;
using Telimena.TestUtilities.Base;
using Telimena.WebApp.UiStrings;
using Assert = NUnit.Framework.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using TestContext = NUnit.Framework.TestContext;

namespace Telimena.WebApp.UITests
{
    [TestFixture]
    public abstract partial class WebsiteTestBase : IntegrationTestBase
    {
        protected Guid RegisterApp(string name, Guid? key, string description, string assemblyName, bool canAlreadyExist, bool hasToExistAlready)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(25));

            this.Driver.FindElement(By.Id(Strings.Id.RegisterApplicationLink)).ClickWrapper(this.Driver);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));
            if (key != null)
            {
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).Clear();
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).SendKeys(key.ToString());
            }
            else
            {
                IWebElement ele = this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox));

                string autoGeneratedGuid = ele.GetAttribute("value");
                Assert.AreNotEqual(Guid.Empty, Guid.Parse(autoGeneratedGuid));
                key = new Guid(autoGeneratedGuid);
            }
            
            this.Driver.FindElement(By.Id(Strings.Id.ProgramNameInputBox)).SendKeys(name);
            this.Driver.FindElement(By.Id(Strings.Id.ProgramDescriptionInputBox)).SendKeys(description);
            this.Driver.FindElement(By.Id(Strings.Id.PrimaryAssemblyNameInputBox)).SendKeys(assemblyName);

            this.Driver.FindElement(By.Id(Strings.Id.SubmitAppRegistration)).ClickWrapper(this.Driver);


            IAlert alert = this.Driver.WaitForAlert(10000);
            if (alert != null)
            {
                if (canAlreadyExist)
                {
                    if (alert.Text != "Use different telemetry key")
                    {
                        Assert.AreEqual($"A program with name [{name}] was already registered by TelimenaSystemDevTeam", alert.Text);
                    }
                    alert.Accept();
                    return key.Value;
                }
                else
                {
                    Assert.Fail("Test scenario expects that the app does not exist");
                }
            }
            else
            {
                if (hasToExistAlready)
                {
                    Assert.Fail("The app should already exist and the error was expected");
                }
            }

            IWebElement programTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));

            var infoElements = programTable.FindElements(By.ClassName(Strings.Css.ProgramInfoElement));

            Assert.AreEqual(name, infoElements[0].Text);
            Assert.AreEqual(description, infoElements[1].Text);
            Assert.AreEqual(key.ToString(), infoElements[2].Text);
            Assert.AreEqual(assemblyName, infoElements[5].Text);
            return key.Value;
        }

        protected void LogOut()
        {
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/LogOff"));
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
        }
     
        protected void ClickOnProgramMenuButton(string appName, string buttonSuffix)
        {
            try
            {
                Retrier.RetryAsync(() =>
                {
                    WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                    this.ExpandAppMenu(appName);

                    IWebElement link = this.Driver.TryFind(By.Id(appName + buttonSuffix));
                    IntegrationTestBase.Log($"Found { appName}{buttonSuffix} button");

                    link = wait.Until(ExpectedConditions.ElementToBeClickable(link));

                    link.ClickWrapper(this.Driver);
                }, TimeSpan.FromMilliseconds(500), 3).GetAwaiter().GetResult();

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to click on button {buttonSuffix} for app {appName}.", ex);
            }
        }



        protected IWebElement TryFindAppMenu(string appName)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            IWebElement element = this.Driver.TryFind(By.Id($"{appName}_menu"));
            if (element == null || !element.Displayed)
            {
                var devTeamElement = this.Driver.TryFind(By.Id($"{Strings.Id.AppsList}_TelimenaSystemDevTeam"));
                wait.Until(ExpectedConditions.ElementToBeClickable(devTeamElement));
                devTeamElement.ClickWrapper(this.Driver);
                IntegrationTestBase.Log($"{appName}_menu was not visible. Clicked on team node to expand.");
            }

            element = this.Driver.TryFind(By.Id($"{appName}_menu"));
            if (element != null)
            {
                IntegrationTestBase.Log($"Found {appName}_menu button");
            }
            else
            {
                IntegrationTestBase.Log($"Did not find {appName}_menu button");
            }

            return element;
        }
        protected IWebElement ExpandAppMenu(string appName)
        {
            var element = this.TryFindAppMenu(appName);

            element.ClickWrapper(this.Driver);
            IntegrationTestBase.Log($"Clicked {appName}_menu button");
            return element;
        }

        protected void ClickOnManageProgramMenu(string appName)
        {
            this.ClickOnProgramMenuButton(appName, "_manageLink");
        }

        protected void NavigateToManageProgramMenu(Guid key)
        {
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl($"ProgramManagement?telemetryKey={key}"));
        }

        protected void NavigateToStatisticsPage(Guid key)
        {
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl($"ProgramStatistics?telemetryKey={key}"));
        }

        public void GoToAdminHomePage()
        {
            try
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
                this.LoginAdminIfNeeded();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while logging in admin", ex);
            }
        }

        public void RecognizeAdminDashboardPage()
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            if (this.Driver.Url.Contains("ChangePassword"))
            {
                IntegrationTestBase.Log("Going from change password page to Admin dashboard");
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
            }

            wait.Until(x => x.FindElement(By.Id(Strings.Id.PortalSummary)));
        }

        public void LoginAdminIfNeeded()
        {
            this.LoginIfNeeded(this.AdminName, this.AdminPassword);
        }

        protected void LoginIfNeeded(string userName, string password)
        {
            if (!this.IsLoggedIn())
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Login"));
            }

            if (this.Driver.Url.IndexOf("Login", StringComparison.InvariantCultureIgnoreCase) != -1 &&

                this.Driver.FindElement(By.Id(Strings.Id.LoginForm)) != null)
            {
                IntegrationTestBase.Log("Trying to log in...");
                IWebElement login = this.Driver.FindElement(By.Id(Strings.Id.Email));

                if (login != null)
                {
                    IWebElement pass = this.Driver.FindElement(By.Id(Strings.Id.Password));
                    login.SendKeys(userName);
                    pass.SendKeys(password);
                    IWebElement submit = this.Driver.FindElement(By.Id(Strings.Id.SubmitLogin));
                    submit.ClickWrapper(this.Driver);
                    this.GoToAdminHomePage();
                    this.RecognizeAdminDashboardPage();
                }
            }
            else
            {
                IntegrationTestBase.Log("Skipping logging in");
            }
        }
    }
}