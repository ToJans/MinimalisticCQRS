namespace MinimalisticCQRS.Commands
{
    public class RegisterAccount
    {
        public string OwnerName { get; set; }

        public string AccountNumber { get; set; }

        public string AccountId { get; set; }
    }
}