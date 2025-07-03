namespace RealEstate.Models;


public class EscrowTransaction
{
    public Guid Id { get; set; }
    public Guid EscrowAccountId { get; set; }
    public EscrowAccount EscrowAccount { get; set; }
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow; public bool IsDebit { get; set; }
}



public class EscrowAccount
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public ApplicationUser User { get; set; }
    public decimal Balance { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
    public ICollection<EscrowTransaction> Transactions { get; set; }
}
