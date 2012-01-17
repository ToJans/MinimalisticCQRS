using System.Collections.Generic;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Domain
{
    // the validator is a saga that can throw on an event in case of eventual consistency
    // however, as this should rarely happen, this approach is chosen over the classical
    // Request/respond approach (i.e. lesser effort to cover most of the cases, opt to
    // resolve the remainder by manual intervention)
    public class AccountUniquenessSaga
    {
        dynamic bus;

        public AccountUniquenessSaga(dynamic bus)
        {
            this.bus = bus;
        }

        List<string> RegisteredAccountNumbers = new List<string>();

        public void CanRegisterAccount(string OwnerName, string AccountNumber, string AccountId)
        {
            Guard.Against(RegisteredAccountNumbers.Contains(AccountNumber), "This account number has already been registered");
        }

        // Might fail due to eventual consistency
        void OnAccountRegistered(string OwnerName, string AccountNumber, string AccountId)
        {
            if (RegisteredAccountNumbers.Contains(AccountNumber))
                // would post an email to the service desk for example
                bus.ReportIssueToBackoffice("Account registration", "Duplicate AccountNumber", new {OwnerName,AccountNumber,AccountId }); 
            else
                RegisteredAccountNumbers.Add(AccountNumber);
        }
    }
}