using Microsoft.EntityFrameworkCore;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;

namespace RealEstate.Repository;

public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<List<PropertiesReturn>> GetAllPropertiesAsync();
    Task<IEnumerable<Property>> GetPropertiesBySellerAsync(string sellerId);
    Task<Property> PreparePropertyFromDtoAsync(PropertyDto dto, string sellerId);
    Task<Property> PreparePropertyFromDtoAsync(PropertyDtoII dto, string sellerId);
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

    public async Task<List<PropertiesReturn>> GetAllPropertiesAsync()
    {
       var ret = await _context.Properties
    .Include(p => p.Images)
    .Select(p => new PropertiesReturn
    {
        Id = p.Id.ToString(),
        Title = p.Title,
        Image = p.DefaultImage,
        Price = p.Price.ToString("N0"), // formatted with commas, e.g., 12,500,000
        Location = p.Location,
        PropertyType = p.PropertyType.ToString(),
        LandType=p.LandType.ToString(),
        BuildingType=p.BuildingType.ToString(),
        Images = p.Images.Select(img => img.ImageUrl).ToList(),
        RemainingImages = p.Images.Count
    })
    .ToListAsync();
        return ret;
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
    public async Task<Property> PreparePropertyFromDtoAsync(PropertyDtoII dto, string sellerId)
    {
        try
        {
            var property = new Property
            {
                Title = dto.Title,
                DefaultImage = dto.DefaultImage,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                PropertyType = (PropertyType)dto.PropertyType,
                LandType = (LandType)dto.LandType,
                BuildingType = (BuildingType)dto.BuildingType,
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                Images = dto.Images.Select(url => new PropertyImage { ImageUrl = url.ImageUrl }).ToList()
            };
            return property;
        }
        catch (Exception ex)
        {

            throw;
        }

    }


}