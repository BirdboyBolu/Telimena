﻿// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System.Linq;
using Moq;
using NUnit.Framework;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion


    [TestFixture]
    public class TestConstructorAndHelperAssembliesLoad
    {
        [Test]
        public void TestInitialize_AssemblyParameter()
        {
            Client.Telimena telimena = new Client.Telimena(this.GetType().Assembly);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);

            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByAssembly()
        {
            Client.Telimena telimena = new Client.Telimena();
            telimena.LoadHelperAssemblies(this.GetType().Assembly, typeof(Capture).Assembly);
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_LoadHelperAssemblies_ByString()
        {
            Client.Telimena telimena = new Client.Telimena();
            telimena.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");
            Assert.AreEqual(2, telimena.ProgramInfo.HelperAssemblies.Count);
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Telimena.Client.Tests"));
            Assert.AreEqual(1, telimena.ProgramInfo.HelperAssemblies.Count(x => x.Name == "Moq"));
            Assert.IsTrue(telimena.ProgramInfo.HelperAssemblies.All(x => x.Version != null && x.Name != null));
        }

        [Test]
        public void TestInitialize_NoParameters()
        {
            Client.Telimena telimena = new Client.Telimena();
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }

        [Test]
        public void TestInitialize_ProgramInfo()
        {
            ProgramInfo pi = new ProgramInfo
            {
                Name = "An App!",
                PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)
            };
            Client.Telimena telimena = new Client.Telimena(pi);
            Assert.AreEqual("An App!", telimena.ProgramInfo.Name);
            Assert.AreEqual("Telimena.Client.Tests", telimena.ProgramInfo.PrimaryAssembly.Name);
            Assert.IsNotNull(telimena.ProgramInfo.PrimaryAssembly.Version);
            Assert.IsNotNull(telimena.UserInfo.UserName);
            Assert.IsNotNull(telimena.UserInfo.MachineName);
        }
    }
}