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

