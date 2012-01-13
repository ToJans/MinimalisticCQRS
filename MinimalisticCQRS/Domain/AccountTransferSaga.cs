
namespace MinimalisticCQRS.Domain
{
    public class AccountTransferSaga
    {
        private dynamic bus;

        public AccountTransferSaga(dynamic bus)
        {
            this.bus = bus;
        }

        public void OnTransferProcessedOnSource(decimal Amount, string TargetAccountId, string AccountId)
        {
            bus.ProcessTransferOnTarget(Amount, SourceAccountId: AccountId, AccountId: TargetAccountId);
        }

        public void OnTransferFailedOnTarget(string Reason, decimal Amount, string SourceAccountId, string AccountId)
        {
            bus.CancelTransfer(Reason,Amount, TargetAccountId:AccountId, AccountId: SourceAccountId);
        }
    }
}