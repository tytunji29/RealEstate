using RealEstate.Helpers;
using System.ComponentModel.DataAnnotations;

namespace RealEstate.Models.ResponseDto;
public class PropertyDto
{
    // public Guid Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Location { get; set; }
    public string Bvn { get; set; }
    public string Nin { get; set; }
    public IFormFile DefaultImage { get; set; }
    public PropertyType PropertyType { get; set; }
    public LandType? LandType { get; set; }
    public BuildingType? BuildingType { get; set; }
    public List<IFormFile> ImageUrls { get; set; }
    // public string SellerId { get; set; }
}
public class PropertyDecisionDto
{
    public string Status { get; set; }
}

//public class PropertyImageDto
//{
//    public string Id { get; set; }
//    public string PropertyId { get; set; }
//    public string ImageUrl { get; set; }
//    public string Property { get; set; }
//}

//public class PropertyDtoII
//{
//    public string Title { get; set; }
//    public string Description { get; set; }
//    public decimal Price { get; set; }
//    public string Location { get; set; }
//    public string Bvn { get; set; }
//    public string Nin { get; set; }
//    public string DefaultImage { get; set; }
//    public int PropertyType { get; set; }
//    public int LandType { get; set; }
//    public int BuildingType { get; set; }
//    public List<PropertyImageDto> ImageUrls { get; set; }
//}
public class Image
{
    public string Id { get; set; }
    public string ImageUrl { get; set; }
    public string PropertyId { get; set; }
    public object Property { get; set; }
}

public class PropertyDtoII
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public string DefaultImage { get; set; }
    public int Price { get; set; }
    public string Location { get; set; }
    public int PropertyType { get; set; }
    public int LandType { get; set; }
    public int BuildingType { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<Image> Images { get; set; }
    public string SellerId { get; set; }
    public object Seller { get; set; }
    public object PropertyViewers { get; set; }
}


public class PropertiesReturn
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Image { get; set; }
    public string Price { get; set; }
    public string Location { get; set; }
    public DateTime CreatedAt { get; set; }
    public string PropertyType { get; set; }
    public string LandType { get; set; }
    public string SellerPhoneNumber { get; set; }
    public string SellerFullName { get; set; }
    public string BuildingType { get; set; }
    public List<string> Images { get; set; }
    public int RemainingImages { get; set; }
}