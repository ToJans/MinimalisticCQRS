using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SignalR.Hubs;
using System.Linq.Expressions;
using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS
{
    public class Account : AR
    {
        decimal Balance = 0;
        bool IsEnabed = false;

        public void RegisterAccount(string OwnerName, string AccountNumber)
        {
            if (IsEnabed) return;
            Apply.AccountRegistered(OwnerName, AccountNumber, AccountId: Id);
        }

        public void DepositCash(decimal Amount)
        {
            Guard.Against(!IsEnabed, "You can not deposit into an unregistered account");
            Guard.Against(Amount < 0, "You can not deposit an amount < 0");
            Apply.AmountDeposited(Amount, AccountId: Id);
        }

        public void WithdrawCash(decimal Amount)
        {
            Guard.Against(!IsEnabed, "You can not withdraw from an unregistered account");
            Guard.Against(Amount < 0, "You can not withdraw an amount < 0");
            Guard.Against(Amount > Balance, "You can not withdraw an amount larger then the current balance");
            Apply.AmountWithdrawn(Amount, AccountId: Id);
        }

        public void TransferAmount(decimal Amount, string TargetAccountId)
        {
            Guard.Against(!IsEnabed, "You can not transfer from an unregistered account");
            Guard.Against(Amount < 0, "You can not transfer an amount < 0");
            Guard.Against(Amount > Balance, "You can not transfer an amount larger then the current balance");
            Apply.AmountWithdrawn(Amount, AccountId: Id);
            Apply.TransferApprovedOnSource(Amount, TargetAccountId, AccountId: Id);
        }

        public void CompleteTransferOnTarget(decimal Amount, string SourceAccountId)
        {
            if (IsEnabed)
                Apply.AmountDeposited(Amount, AccountId: Id);
            else
            {
                Apply.TransferFailedOnTarget(Amount, SourceAccountId, AccountId: Id);
                Guard.Against(!IsEnabed, "You can not transfer to an unregistered account");
            }
        }

        public void CancelTransferOnSource(decimal Amount)
        {
            Apply.AmountDeposited(Amount, AccountId: Id);
        }


        // events
        void OnAccountRegistered(string OwnerName)
        {
            Balance = 0;
            IsEnabed = true;
        }

        void OnAmountDeposited(decimal Amount)
        {
            Balance += Amount;
        }

        void OnAmountWithdrawn(decimal Amount)
        {
            Balance -= Amount;
        }

    }

    public class AccountTransferSaga
    {
        private dynamic bus;

        public AccountTransferSaga(dynamic bus)
        {
            this.bus = bus;
        }

        public void OnTransferApprovedOnSource(decimal Amount, string TargetAccountId, string AccountId)
        {
            bus.CompleteTransferOnTarget(Amount, SourceAccountId: AccountId, AccountId: TargetAccountId);
        }

        public void OnTransferFailedOnTarget(decimal Amount, string SourceAccountId, string AccountId)
        {
            bus.CancelTransferOnSource(Amount, AccountId: SourceAccountId);
        }
    }

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