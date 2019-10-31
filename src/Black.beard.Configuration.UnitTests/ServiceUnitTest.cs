using Bb.Configuration.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Black.beard.Configuration.UnitTests
{
    [TestClass]
    public class ServiceUnitTest
    {

        [TestMethod]
        public void TestAddAccount()
        {

            OptionService service = GetService();

            var account1 = service.AddAccount("account1", "toto@test.com");
            var appl1 = service.GetAccounts().FirstOrDefault(c => c.Id == account1.Id);
            Assert.AreEqual(appl1, account1);
            service.RemoveAccount(account1);
            var srv = service.GetAccounts().FirstOrDefault(c => c.Id == account1.Id);
            Assert.AreEqual(srv, null);

        }

        [TestMethod]
        public void TestAddApplication()
        {

            OptionService service = GetService();

            var account1 = service.AddAccount("account1", "toto@test.com");

            var application1 = service.AddApplication(account1.Id, "application1");
            var appl1 = service.GetApplications(account1.Id).FirstOrDefault(c => c.Id == application1.Id);
            Assert.AreEqual(appl1, application1);
            service.DeleteApplication(application1);
            var srv = service.GetApplications(account1.Id).FirstOrDefault(c => c.Id == application1.Id);
            Assert.AreEqual(srv, null);

        }

        [TestMethod]
        public void TestAddEnvironment()
        {

            OptionService service = GetService();

            var account1 = service.AddAccount("account1", "toto@test.com");

            var env1 = service.AddEnvironment(account1.Id, "env1");
            var env2 = service.GetEnvironments(account1.Id).FirstOrDefault(c => c.Id == env1.Id);
            Assert.AreEqual(env2, env1);
            service.DeleteEnvironment(env1);
            var srv = service.GetEnvironments(account1.Id).FirstOrDefault(c => c.Id == env1.Id);
            Assert.AreEqual(srv, null);

        }

        [TestMethod]
        public void TestAddType()
        {

            OptionService service = GetService();
            var account1 = service.AddAccount("account1", "toto@test.com");

            var type1 = service.AddType(account1.Id, "type", ".json", "Validator", "");
            var env2 = service.GetTypes(account1.Id).FirstOrDefault(c => c.Id == type1.Id);
            Assert.AreEqual(env2, type1);
            service.DeleteType(type1);
            var srv = service.GetTypes(account1.Id).FirstOrDefault(c => c.Id == type1.Id);
            Assert.AreEqual(srv, null);

        }

        [TestMethod]
        public void TestAddConfig()
        {

            OptionService service = GetService();
            var account1 = service.AddAccount("account1", "toto@test.com");

            var appli = service.AddApplication(account1.Id, "appl1");
            var environment = service.AddEnvironment(account1.Id, "env1");
            var type = service.AddType(account1.Id, "type1", ".json", "validation1", "");

            service.AddOption(account1.Id, "appl1", "env1", "type1", "maconfig", "maconfig");


            var i = service.Option(account1.Id, "appl1", "env1", "maconfig", "type1");



        }

        //private static OptionService GetService()
        //{

        //    var configuration = new OptionConfiguration()
        //    {
        //        ConnectionString = @"mongodb://localhost:27017/Test",
        //        DatabaseName = "Test",
        //    };

        //    var service = new OptionService(configuration);
        //    return service;

        //}

    }
}
