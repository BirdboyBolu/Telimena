﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AutomaticTestsClient;
using DotNetLittleHelpers;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests.IntegrationTests.BackwardCompatibilityIntegrationTests
{
    public class VersionTuple
    {
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }



    [TestFixture(), Timeout(3*60*1000)]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        public async Task<VersionTuple> GetVersionsFromApp(string appName, string testSubfolderName)
        {
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName);
            return await this.GetVersionsFromApp(exe);
        }
        public async Task<VersionTuple> GetVersionsFromApp(FileInfo exe)
        {
            Log($"Starting process [{exe.FullName}]");
            var process = Process.Start(exe.FullName);
            Application app = TestStack.White.Application.Attach(process);

            Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run"), TimeSpan.FromMinutes(2));
            return await this.GetVersionFromMsgBox(appWarning);
        }

        public async Task<VersionTuple> GetVersionFromMsgBox(Window msgBox)
        {
            string text = msgBox.Get<Label>(SearchCriteria.Indexed(0)).Text;
            string[] versions = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            msgBox.Get<Button>(SearchCriteria.ByText("OK")).Click();
            await Task.Delay(500);
            return new VersionTuple() { AssemblyVersion = versions[0].Trim().Replace("AssemblyVersion: ",""), FileVersion = versions[1].Trim().Replace("FileVersion: ", "") };
        }

         [Test]
        public async Task HandleUpdatesNonBetaTests()
        {
            try
            {

                VersionTuple initialVersions = await this.GetVersionsFromApp(TestAppProvider.FileNames.TestAppV1, nameof(this.HandleUpdatesNonBetaTests));

                FileInfo appFile;

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(out appFile, Actions.HandleUpdates, TestAppProvider.FileNames.TestAppV1
                    , nameof(this.HandleUpdatesNonBetaTests), waitForExit: false);
                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient update installation"), TimeSpan.FromMinutes(1));
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("Updater"), TimeSpan.FromMinutes(1));
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();

                Window doneMsg = await TestHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1));
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2));
                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning);

                Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion));
                Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion));
                VersionTuple postUpdateVersions = await this.GetVersionsFromApp(appFile);
                Assert.AreEqual(postUpdateVersions.AssemblyVersion, newVersions.AssemblyVersion);
                Assert.AreEqual(postUpdateVersions.FileVersion, newVersions.FileVersion);

                //now just assert that the update check result is empty next time
                var result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdates, waitForExit:true);

                Assert.IsFalse(result.IsUpdateAvailable);
                Assert.IsNull(result.Exception);
            }
            catch (Exception ex)
            {
                throw this.Rethrow(ex);
            }
        }


        [Test]
        public async Task HandlePackageUpdatesNonBetaTests()
        {
            try
            {
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, TestAppProvider.FileNames.PackageUpdaterTestAppV1, nameof(this.HandlePackageUpdatesNonBetaTests), waitForExit: false);

                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update installation"), TimeSpan.FromMinutes(1));
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window executed = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater executed"), TimeSpan.FromMinutes(1));
                executed.Get<Button>(SearchCriteria.ByText("OK")).Click();

                Window doneMsg = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater finished"), TimeSpan.FromMinutes(1));
                var text = doneMsg.Get<Label>();
                Assert.AreEqual("Killed other processes: True", text.Text);
                doneMsg.Get<Button>(SearchCriteria.ByText("OK")).Click();

                //do not check if app was updated, because we only care whether the updater was actually launched

            }
            catch (Exception ex)
            {
                throw this.Rethrow(ex);
            }
            
        }
    }
}
