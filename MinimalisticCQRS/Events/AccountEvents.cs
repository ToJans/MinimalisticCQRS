namespace MinimalisticCQRS.Events
{
    public class AccountRegistered
    {
        public string OwnerName { get; set; }

        public string AccountNumber { get; set; }

        public string AccountId { get; set; }
    }

    public class AmountDeposited
    {
        public decimal Amount { get; set; }

        public string AccountId { get; set; }
    }
}