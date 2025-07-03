using System.ComponentModel.DataAnnotations;
using RealEstate.Helpers;

namespace RealEstate.Models;

public class Property
{
    public Guid Id { get; set; }
    [Required] public string Title { get; set; }
    public string Description { get; set; }
    public string DefaultImage { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; }
    public PropertyType PropertyType { get; set; }
    public LandType? LandType { get; set; }
    public BuildingType? BuildingType { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<PropertyImage> Images { get; set; }
    public string SellerId { get; set; }
    public ApplicationUser Seller { get; set; }
    public ICollection<PropertyViewers> PropertyViewers { get; set; }
}

public class UnverifiedSellerProduct
{
    public Guid Id { get; set; } 
    public string SellerId { get; set; }
    public string Data { get; set; }
}

public class PropertyViewers
{
    public Guid Id { get; set; }
    public string ViewerId { get; set; }
    public ApplicationUser Viewer { get; set; }
    public Guid PropertyId { get; set; }
    public Property Property { get; set; }
    public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    public bool PaymentMade { get; set; } = false;
}

