using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Hubs
{
    public class CommandHub : Hub
    {
        dynamic bus;

        public CommandHub(dynamic bus)
        {
            this.bus = bus;
        }

        public void RegisterAccount(string OwnerName, string AccountNumber, string AccountId)
        {
            bus.RegisterAccount(OwnerName, AccountNumber, AccountId: AccountId);
        }

        public void DepositCash(decimal Amount, string AccountId)
        {
            bus.DepositCash(Amount, AccountId: AccountId);
        }

        public void WithdrawCash(decimal Amount, string AccountId)
        {
            bus.WithdrawCash(Amount, AccountId: AccountId);
        }

        public void TransferAmount(decimal Amount, string TargetAccountId, string AccountId)
        {
            bus.TransferAmount(Amount, TargetAccountId, AccountId:AccountId);
        }

        // commands don't need to be executed by AR if they are irrelevant to the domain
        public void ShareMessage(string username, string message)
        {
            Guard.Against(string.IsNullOrWhiteSpace(username), "Username can not be empty");
            if (string.IsNullOrWhiteSpace(message))
                message = "ZOMG!!! I have no idea what to say, so I'll just say this stuff has lots of awesomesauce";
            bus.OnMessageShared(username, message);
        }

    }
}