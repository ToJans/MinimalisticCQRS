
namespace MinimalisticCQRS.Domain
{
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
}