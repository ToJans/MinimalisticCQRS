using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using System.Text;

namespace MinimalisticCQRS.Hubs
{
    public class QueryHub : Hub
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

        void OnAccountRegistered(string OwnerName, string AccountNumber, string AccountId)
        {
            var detail = new AccountDetails
            {
                Id = AccountId,
                AccountNumber = AccountNumber,
                OwnerName = OwnerName,
                Balance = 0
            };
            Details.Add(AccountId, detail);
            Clients.AddAccountDetails(detail);
        }

        void OnAmountDeposited(decimal Amount, string AccountId)
        {
            Details[AccountId].Balance += Amount;
            Clients.UpdateBalance(Details[AccountId].Balance,AccountId);
        }

        void OnAmountWithdrawn(decimal Amount, string AccountId)
        {
            Details[AccountId].Balance -= Amount;
            Clients.UpdateBalance( Details[AccountId].Balance, AccountId);
        }

        void OnMessageShared(string username, string message)
        {
            Clients.AddChatMessage(username, message);
        }

        void OnTransferCanceled(string Reason, decimal Amount, string TargetAccountId, string AccountId)
        {
            Caller.Alert(Reason);
        }

        // a direct command
        void ReportIssueToBackoffice(string subject, string details, object data)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("An error occured; normally this message would have been reported to the "+
                "backoffice, and they would manually resolve this issue; in this case I just show an alert");
            sb.AppendLine("MailTo: \tSupport@blah.com");
            sb.Append("Subject:\t");
            sb.AppendLine(subject);
            sb.Append("Body:\t");
            sb.AppendLine(details);
            sb.Append("Attachment:\t");
            //sb.AppendLine(data); // as JSON

            Caller.Alert(sb.ToString());
        }
    }
}