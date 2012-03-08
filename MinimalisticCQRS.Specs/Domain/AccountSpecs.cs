using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MinimalisticCQRS.Commands;
using MinimalisticCQRS.Domain;
using MinimalisticCQRS.Events;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Specs.Domain
{
    [TestClass]
    public class AccountSpecs
    {
        private SpecBus SUT;

        [TestInitialize]
        public void DefineScope()
        {
            var mvr = new MiniVanRegistry();
            SUT = new SpecBus(mvr);
            mvr.RegisterArType<Account>();
        }

        [TestMethod]
        public void Registering_an_account_and_verifying_an_event_with_a_function_call_should_succeed()
        {
            SUT.WhenCommand(x => x.RegisterAccount(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));

            SUT.ThenExpectEvent(x => x.AccountRegistered(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));
        }

        [TestMethod]
        public void Registering_an_account_with_a_command_class_and_verifying_events_with_a_function_call_should_succeed()
        {
            SUT.WhenCommand(new RegisterAccount
            {
                OwnerName = "Tom Janssens",
                AccountNumber = "123-456789-01",
                AccountId = "account/1"
            });

            SUT.ThenExpectEvent(x => x.AccountRegistered(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));
        }

        [TestMethod]
        public void Registering_an_account_and_verifying_with_an_event_class_should_succeed()
        {
            SUT.WhenCommand(x => x.RegisterAccount(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));

            SUT.ThenExpectEvent(new AccountRegistered { OwnerName = "Tom Janssens", AccountNumber = "123-456789-01", AccountId = "account/1" });
        }

        [TestMethod]
        public void Registering_an_account_twice_should_do_nothing()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));

            SUT.WhenCommand(x => x.RegisterAccount(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));

            SUT.ThenDoNotExpectEvent(x => x.AccountRegistered(OwnerName: "Tom Janssens", AccountNumber: "123-456789-01", AccountId: "account/1"));
        }

        [TestMethod]
        public void Depositing_cash_to_a_registered_account_should_succeed()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));

            SUT.WhenCommand(x => x.DepositCash(Amount: 10m, AccountId: "account/1"));

            SUT.ThenExpectEvent(x => x.AmountDeposited(Amount: 10m, AccountId: "account/1"));
        }

        [TestMethod]
        public void Withdrawing_cash_from_a_registered_account_with_enough_money_should_succeed()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
            SUT.GivenEvent(x => x.AmountDeposited(Amount: 10000m, AccountId: "account/1"));

            SUT.WhenCommand(x => x.WithdrawCash(Amount: 10m, AccountId: "account/1"));

            SUT.ThenExpectEvent(x => x.AmountWithdrawn(Amount: 10m, AccountId: "account/1"));
        }

        [TestMethod]
        public void Withdrawing_cash_from_a_registered_account_with_not_enough_money_should_fail()
        {
            SUT.GivenEvent(x => x.AccountRegistered("Tom Janssens", "123-456789-01", AccountId: "account/1"));
            SUT.GivenEvent(x => x.AmountDeposited(Amount: 5m, AccountId: "account/1"));

            SUT.WhenCommand(x => x.WithdrawCash(Amount: 10m, AccountId: "account/1"));

            SUT.ThenExpectException<InvalidOperationException>(x => x.Message.Contains("larger then"));
        }
    }
}