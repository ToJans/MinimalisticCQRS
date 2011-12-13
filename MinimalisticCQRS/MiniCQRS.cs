using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace MinimalisticCQRS
{
    public interface IHandleChanges
    {
        void OnAccountRegistered(string AccountId, string OwnerName,string AccountNumber);
        void OnAmountDeposited(string AccountId, decimal Amount);
        void OnAmountWithDrawn(string AccountId, decimal Amount);
        void OnMessageShared(string username, string message);
    }

    public class CommandHub : Hub,IHandleChanges
    {
        IHandleChanges[] EventAppliers;

        List<string> RegisteredAccountNumbers = new List<string>();
        Dictionary<string, decimal> AccountBalances = new Dictionary<string, decimal>();

        public CommandHub(IHandleChanges EventAppliers)
        {
            this.EventAppliers = new IHandleChanges[] {this,EventAppliers};
        }

        // commands
        public void RegisterAccount(string AccountId, string OwnerName,string AccountNumber)
        {
            Guard.Against(RegisteredAccountNumbers.Contains(AccountNumber), "This account number has already been registered");
            Apply(x=>x.OnAccountRegistered(AccountId,OwnerName,AccountNumber));
        }

        public void DepositAmount(string AccountId, decimal Amount)
        {
            Guard.Against(!AccountBalances.ContainsKey(AccountId), "You can not deposit into an unregistered account");
            Guard.Against(Amount < 0, "You can not deposit an amount < 0");
            Apply(x=>x.OnAmountDeposited(AccountId,Amount));
        }

        public void WithdrawAmount(string AccountId, decimal Amount)
        {
            Guard.Against(!AccountBalances.ContainsKey(AccountId), "You can not withdraw from an unregistered account");
            Guard.Against(Amount < 0, "You can not withdraw an amount < 0");
            Guard.Against(Amount > AccountBalances[AccountId], "You can not withdraw an amount larger then the current balance");
            Apply(x => x.OnAmountWithDrawn(AccountId, Amount));
        }

        public void ShareMessage(string username, string message)
        {
            Guard.Against(string.IsNullOrWhiteSpace(username), "Username can not be empty");
            if (string.IsNullOrWhiteSpace(message))
                message = "ZOMG!!! I have no idea what to say, so I'll just say this stuff has lots of awesomesauce";
            Apply(x=>x.OnMessageShared(username,message));
        }

        // events
        void IHandleChanges.OnAccountRegistered(string AccountId, string OwnerName, string AccountNumber)
        {
            RegisteredAccountNumbers.Add(AccountNumber);
            AccountBalances.Add(AccountId, 0);
        }

        void IHandleChanges.OnAmountDeposited(string AccountId, decimal Amount)
        {
            AccountBalances[AccountId] += Amount;
        }

        void IHandleChanges.OnAmountWithDrawn(string AccountId, decimal Amount)
        {
            AccountBalances[AccountId] -= Amount;
        }

        void IHandleChanges.OnMessageShared(string username, string message)
        {
            // don't do anything at all
        }

        // helpers
        void Apply(Action<IHandleChanges> action)
        {
            foreach (var c in EventAppliers) action(c);
        }

        static class Guard
        {
            public static void Against(bool assertion, string message)
            {
                if (assertion) throw new InvalidOperationException(message);
            }
        }
    }

    public class QueryHub : Hub,IHandleChanges
    {
        public class AccountDetails
        {
            public string Id;
            public string OwnerName;
            public Decimal Balance;
            public string AccountNumber;
            public string Description { get { return string.Format("{0} - {1}", AccountNumber, OwnerName); } }
        }

        Dictionary<string, AccountDetails> Details;

        public QueryHub()
        {
            Details = new Dictionary<string, AccountDetails>();
        }

        public AccountDetails[] GetDetails()
        {
            return Details.Values.ToArray();
        }

        void IHandleChanges.OnAccountRegistered(string AccountId, string OwnerName, string AccountNumber)
        {
            var detail = new AccountDetails { 
                Id = AccountId,
                AccountNumber = AccountNumber,
                OwnerName = OwnerName,
                Balance = 0
            };
            Details.Add(AccountId, detail);
            Clients.AddAccountDetails(detail);
        }

        void IHandleChanges.OnAmountDeposited(string AccountId, decimal Amount)
        {
            Details[AccountId].Balance += Amount;
            Clients.UpdateBalance(AccountId, Details[AccountId].Balance);
        }

        void IHandleChanges.OnAmountWithDrawn(string AccountId, decimal Amount)
        {
            Details[AccountId].Balance -= Amount;
            Clients.UpdateBalance(AccountId, Details[AccountId].Balance);
        }

        void IHandleChanges.OnMessageShared(string username, string message)
        {
            Clients.AddChatMessage(username, message);
        }



    }
}