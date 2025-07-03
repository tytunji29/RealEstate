using Microsoft.EntityFrameworkCore;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;

namespace RealEstate.Repository;

public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<IEnumerable<Property>> GetPropertiesBySellerAsync(string sellerId);
    Task<Property> PreparePropertyFromDtoAsync(PropertyDto dto, string sellerId);
}

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    private readonly RealEstateDbContext _context;

    private readonly IUploadFileService _uploadFileService;
    public PropertyRepository(RealEstateDbContext context, IUploadFileService uploadFileService) : base(context)
    {
        _context = context;
        _uploadFileService = uploadFileService;
    }

    public async Task<IEnumerable<Property>> GetPropertiesBySellerAsync(string sellerId)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Where(p => p.SellerId == sellerId)
            .ToListAsync();
    }

    public async Task<Property> PreparePropertyFromDtoAsync(PropertyDto dto, string sellerId)
    {
        // ✅ Upload default image
        string defImage = await _uploadFileService.UploadImageAsync(dto.DefaultImage, "AgentsDoc");

        // ✅ Upload gallery images
        List<string> imageUrls = await _uploadFileService.UploadImagesAsync(dto.ImageUrls, "AgentsDoc");

        // ✅ Map to Property entity
        var property = new Property
        {
            Title = dto.Title,
            DefaultImage = defImage,
            Description = dto.Description,
            Price = dto.Price,
            Location = dto.Location,
            PropertyType = dto.PropertyType,
            LandType = dto.LandType,
            BuildingType = dto.BuildingType,
            SellerId = sellerId,
            CreatedAt = DateTime.UtcNow,
            Images = imageUrls.Select(url => new PropertyImage { ImageUrl = url }).ToList()
        };

        return property;
    }

}