using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalisticCQRS.Domain;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Specs.Domain
{
    [TestClass]
    public class AccountTransfers
    {
        private SpecBus SUT;

        [TestInitialize]
        public void DefineScope()
        {
            var mvr = new MiniVanRegistry();
            SUT = new SpecBus(mvr);
            mvr.RegisterArType<Account>();
            mvr.RegisterNonArInstance(new AccountTransferSaga(SUT));
        }

        [TestMethod]
        public void Transfering_an_amount_from_a_to_b_should_succeed_if_all_valid()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
            SUT.GivenEvent(x => x.AmountDeposited(Amount: 100m, AccountId: "account/1"));
            SUT.GivenEvent(x => x.AccountRegistered("Liesbeth kint", "223-456789-01", AccountId: "account/2"));

            SUT.WhenCommand(x => x.TransferAmount(Amount: 10m, TargetAccountId: "account/2", AccountId: "account/1"));

            SUT.ThenExpectEvent(x => x.AmountWithdrawn(Amount: 10m, AccountId: "account/1"));
            SUT.ThenExpectEvent(x => x.AmountDeposited(Amount: 10m, AccountId: "account/2"));
        }

        [TestMethod]
        public void Transfering_an_amount_from_a_to_b_should_not_succeed_if_not_enough_money()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
            SUT.GivenEvent(x => x.AmountDeposited(Amount: 5m, AccountId: "account/1"));
            SUT.GivenEvent(x => x.AccountRegistered("Liesbeth kint", "223-456789-01", AccountId: "account/2"));

            SUT.WhenCommand(x => x.TransferAmount(Amount: 10m, TargetAccountId: "account/2", AccountId: "account/1"));

            SUT.ThenDoNotExpectEvent(x => x.AmountWithdrawn(Amount: 10m, AccountId: "account/1"));
            SUT.ThenDoNotExpectEvent(x => x.AmountDeposited(Amount: 10m, AccountId: "account/2"));
        }

        [TestMethod]
        public void Transfering_an_amount_from_a_to_b_should_not_succeed_if_account_b_is_not_valid()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
            SUT.GivenEvent(x => x.AmountDeposited(Amount: 100m, AccountId: "account/1"));

            SUT.WhenCommand(x => x.TransferAmount(Amount: 10m, TargetAccountId: "account/2", AccountId: "account/1"));

            SUT.ThenExpectEvent(x => x.AmountWithdrawn(Amount: 10m, AccountId: "account/1"));
            SUT.ThenExpectEvent(x => x.AmountDeposited(Amount: 10m, AccountId: "account/1"));
            SUT.ThenExpectEvent(x => x.TransferCanceled("You can not transfer to an unregistered account", 10m, "account/2", AccountId: "account/1"));
        }
    }
}