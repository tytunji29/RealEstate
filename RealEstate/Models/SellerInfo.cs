namespace RealEstate.Models;

public class SellerInfo
{
    public Guid Id { get; set; }
    public string UserId { get; set; }
    public string Bvn { get; set; }
    public string Nin { get; set; }
    public string VirtualAccount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; }
    public string ApprovalStatus { get; set; }
    public string ActedOnBy { get; set; }
}
