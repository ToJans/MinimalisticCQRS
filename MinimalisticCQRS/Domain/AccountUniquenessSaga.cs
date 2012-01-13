using System.Collections.Generic;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Domain
{
    public class AccountUniquenessSaga
    {
        List<string> RegisteredAccountNumbers = new List<string>();

        public void CanRegisterAccount(string OwnerName, string AccountNumber, string AccountId)
        {
            Guard.Against(RegisteredAccountNumbers.Contains(AccountNumber), "This account number has already been registered");
        }

        void OnAccountRegistered(string OwnerName, string AccountNumber, string AccountId)
        {
            RegisteredAccountNumbers.Add(AccountNumber);
        }
    }
}