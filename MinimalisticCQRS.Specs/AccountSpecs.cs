using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalisticCQRS.Domain;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Specs
{
    [TestClass]
    public class AccountSpecs
    {
        [TestMethod]
        public void RegisterAnAccount()
        {
            var mvr = new MiniVanRegistry();
            mvr.RegisterArType<Account>();
            var SUT = new SpecBus(mvr);
            SUT.When(x => x.RegisterAccount(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));
            SUT.Then(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
        }
    }
}