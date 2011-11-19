using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;

namespace MinimalisticCQRS
{
    public interface IChange
    {
        void AccountRegistered(string AccountId, string OwnerName,string AccountNumber);
        void AmountDeposited(string AccountId, decimal Amount);
        void AmountWithDrawn(string AccountId, decimal Amount);
        void SomethingSaid(string username, string message);
    }

    public class CommandHub : Hub,IChange
    {
        IChange[] Changers;

        List<string> RegisteredAccountNumbers = new List<string>();
        Dictionary<string, decimal> AccountBalances = new Dictionary<string, decimal>();

        public CommandHub(IChange Changer)
        {
            this.Changers = new IChange[] {this,Changer};
        }

        // commands
        public void RegisterAccount(string AccountId, string OwnerName,string AccountNumber)
        {
            Guard.Against(RegisteredAccountNumbers.Contains(AccountNumber), "This account number has already been registered");
            Apply(x=>x.AccountRegistered(AccountId,OwnerName,AccountNumber));
        }

        public void DepositAmount(string AccountId, decimal Amount)
        {
            Guard.Against(!AccountBalances.ContainsKey(AccountId), "You can not deposit into an unregistered account");
            Guard.Against(Amount < 0, "You can not deposit an amount < 0");
            Apply(x=>x.AmountDeposited(AccountId,Amount));
        }

        public void WithdrawAmount(string AccountId, decimal Amount)
        {
            Guard.Against(!AccountBalances.ContainsKey(AccountId), "You can not withdraw from an unregistered account");
            Guard.Against(Amount < 0, "You can not withdraw an amount < 0");
            Guard.Against(Amount > AccountBalances[AccountId], "You can not withdraw an amount larger then the current balance");
            Apply(x => x.AmountWithDrawn(AccountId, Amount));
        }

        public void SaySomething(string username, string message)
        {
            Guard.Against(string.IsNullOrWhiteSpace(username), "Username can not be empty");
            if (string.IsNullOrWhiteSpace(message))
                message = "ZOMG!!! I have no idea what to say, so I'll just say this stuff has lots of awesomesauce";
            Apply(x=>x.SomethingSaid(username,message));
        }

        // events
        void IChange.AccountRegistered(string AccountId, string OwnerName, string AccountNumber)
        {
            RegisteredAccountNumbers.Add(AccountNumber);
            AccountBalances.Add(AccountId, 0);
        }

        void IChange.AmountDeposited(string AccountId, decimal Amount)
        {
            AccountBalances[AccountId] += Amount;
        }

        void IChange.AmountWithDrawn(string AccountId, decimal Amount)
        {
            AccountBalances[AccountId] -= Amount;
        }

        void IChange.SomethingSaid(string username, string message)
        {
            // don't do anything at all
        }

        // helpers
        void Apply(Action<IChange> action)
        {
            foreach (var c in Changers) action(c);
        }

        class Guard
        {
            public static void Against(bool assertion, string message)
            {
                if (assertion) throw new InvalidOperationException(message);
            }
        }


        public void SomethingSaid(string username, string message)
        {
            // DO nothing
        }
    }

    public class AccountDetails
    {
        public string Id;
        public string OwnerName;
        public Decimal Balance;
        public string AccountNumber;
        public string Description { get { return string.Format("{0} - {1}", AccountNumber, OwnerName); } }
    }

    public class AccountBalances : Dictionary<string, decimal> { }

    public class QueryHub : Hub,IChange
    {
        Dictionary<string, AccountDetails> Details;

        public AccountDetails[] GetDetails()
        {
            return Details.Values.ToArray();
        }

        public QueryHub()
        {
            Details = new Dictionary<string, AccountDetails>();
        }

        void IChange.AccountRegistered(string AccountId, string OwnerName, string AccountNumber)
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

        void IChange.AmountDeposited(string AccountId, decimal Amount)
        {
            Details[AccountId].Balance += Amount;
            Clients.UpdateBalance(AccountId, Details[AccountId].Balance);
        }

        void IChange.AmountWithDrawn(string AccountId, decimal Amount)
        {
            Details[AccountId].Balance -= Amount;
            Clients.UpdateBalance(AccountId, Details[AccountId].Balance);
        }


        void IChange.SomethingSaid(string username, string message)
        {
            Clients.AddChatMessage(username, message);
        }
    }
}