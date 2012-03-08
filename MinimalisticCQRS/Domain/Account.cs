using MinimalisticCQRS.Infrastructure;

namespace MinimalisticCQRS.Domain
{
    public class Account : AR
    {
        decimal Balance = 0;
        bool IsEnabled = false;

        public void RegisterAccount(string OwnerName, string AccountNumber)
        {
            if (IsEnabled) return;
            ApplyEvent.AccountRegistered(OwnerName: OwnerName, AccountNumber: AccountNumber, AccountId: Id);
        }

        public void DepositCash(decimal Amount)
        {
            Guard.Against(!IsEnabled, "You can not deposit into an unregistered account");
            Guard.Against(Amount < 0, "You can not deposit an amount < 0");
            // we support both instance classes and direct calls to emit events
            //Apply.AmountDeposited(Amount:Amount, AccountId: Id);
            Apply(new Events.AmountDeposited { AccountId = Id, Amount = Amount });
        }

        public void WithdrawCash(decimal Amount)
        {
            Guard.Against(!IsEnabled, "You can not withdraw from an unregistered account");
            Guard.Against(Amount < 0, "You can not withdraw an amount < 0");
            Guard.Against(Amount > Balance, "You can not withdraw an amount larger then the current balance");
            ApplyEvent.AmountWithdrawn(Amount: Amount, AccountId: Id);
        }

        public void TransferAmount(decimal Amount, string TargetAccountId)
        {
            Guard.Against(!IsEnabled, "You can not transfer from an unregistered account");
            Guard.Against(Amount < 0, "You can not transfer an amount < 0");
            Guard.Against(Amount > Balance, "You can not transfer an amount larger then the current balance");
            ApplyEvent.AmountWithdrawn(Amount: Amount, AccountId: Id);
            ApplyEvent.TransferProcessedOnSource(Amount, TargetAccountId, AccountId: Id);
        }

        public void ProcessTransferOnTarget(decimal Amount, string SourceAccountId)
        {
            if (IsEnabled)
            {
                ApplyEvent.AmountDeposited(Amount: Amount, AccountId: Id);
                ApplyEvent.TransferCompleted(Amount, SourceAccountId, AccountId: Id);
            }
            else
            {
                ApplyEvent.TransferFailedOnTarget("You can not transfer to an unregistered account", Amount, SourceAccountId, AccountId: Id);
            }
        }

        public void CancelTransfer(string Reason, decimal Amount, string TargetAccountId)
        {
            ApplyEvent.AmountDeposited(Amount: Amount, AccountId: Id);
            ApplyEvent.TransferCanceled(Reason, Amount, TargetAccountId, AccountId: Id);
        }

        // events
        private void OnAccountRegistered(string OwnerName)
        {
            Balance = 0;
            IsEnabled = true;
        }

        private void OnAmountDeposited(decimal Amount)
        {
            Balance += Amount;
        }

        private void OnAmountWithdrawn(decimal Amount)
        {
            Balance -= Amount;
        }
    }
}