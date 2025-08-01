using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;
using RealEstate.RequestDtos;
using static System.Net.Mime.MediaTypeNames;

namespace RealEstate.Repository;

public interface IPropertyRepository : IGenericRepository<Property>
{
    Task<(List<PropertiesReturn>, int)> GetAllPropertiesAsync(int pgNo, int pgSize, string status);
    Task<IEnumerable<Property>> GetPropertiesBySellerAsync(string sellerId);
    Task<Property> PrepareAndSavePropertyAsync(PropertyDto dto, string sellerId);
    Task<bool> UpdateDecision(string Id, string action, string approvalId);
}

public class PropertyRepository : GenericRepository<Property>, IPropertyRepository
{
    private readonly RealEstateDbContext _context;

    private readonly IUploadFileService _uploadFileService;
    private readonly IPinnacleService _apiCall;
    public PropertyRepository(RealEstateDbContext context, IPinnacleService apiCall, IUploadFileService uploadFileService) : base(context)
    {
        _context = context;
        _apiCall = apiCall;
        _uploadFileService = uploadFileService;
    }

    public async Task<(List<PropertiesReturn>, int)> GetAllPropertiesAsync(int pgNo, int pgSize, string status)
    {
        string strStatus = status.ToLower();
        var baseQuery = _context.Properties
            .Where(o => o.ApprovalStatus.ToLower() == strStatus)
            .Include(p => p.Images);

        int totalRecords = await baseQuery.CountAsync();

        var ret = await baseQuery
     .OrderByDescending(p => p.CreatedAt)
     .Skip((pgNo - 1) * pgSize)
     .Take(pgSize)
     .Select(p => new
     {
         p.Id,
         p.Title,
         p.CreatedAt,
         p.DefaultImage,
         p.Price,
         p.Location,
         p.PropertyType,
         p.LandType,
         p.BuildingType,
         Images = p.Images.Select(img => img.ImageUrl).ToList()
     })
     .ToListAsync();
        var result = ret.Select(p => new PropertiesReturn
        {
            Id = p.Id.ToString(),
            Title = p.Title,
            CreatedAt = p.CreatedAt,
            Image = p.DefaultImage,
            Price = p.Price.ToString("N0"),
            Location = p.Location,
            PropertyType = p.PropertyType.ToString(),
            LandType = p.LandType?.ToString(),
            BuildingType = p.BuildingType?.ToString(),

            Images = new[] { p.DefaultImage }
        .Concat(p.Images ?? new List<string>())
        .ToList(),

            RemainingImages = (p.Images?.Count ?? 0) + 1
        }).ToList();


        return (result, totalRecords);
    }


    public async Task<IEnumerable<Property>> GetPropertiesBySellerAsync(string sellerId)
    {
        return await _context.Properties
            .Include(p => p.Images)
            .Where(p => p.SellerId == sellerId)
            .ToListAsync();
    }

    public async Task<Property> PrepareAndSavePropertyAsync(PropertyDto dto, string sellerId)
    {
        try
        {
            // Upload default image
            string defImage = await _uploadFileService.UploadImageAsync(dto.DefaultImage, "AgentsDoc");

            // Upload gallery images
            List<string> imageUrls = await _uploadFileService.UploadImagesAsync(dto.ImageUrls, "AgentsDoc");

            // Create Property with images
            var property = new Property
            {
                Title = dto.Title,
                DefaultImage = defImage,
                Description = dto.Description,
                Price = dto.Price,
                Location = dto.Location,
                ActedOnBy = "",
                ActedOnDate = DateTime.Now,
                PropertyType = dto.PropertyType,
                ApprovalStatus = "Pending",
                LandType = dto.LandType,
                BuildingType = dto.BuildingType,
                SellerId = sellerId,
                CreatedAt = DateTime.UtcNow,
                Images = imageUrls.Select(url => new PropertyImage { ImageUrl = url }).ToList()
            };

            // Add and save in one go
            await _context.Properties.AddAsync(property);
            await _context.SaveChangesAsync();

            return property;

        }
        catch (Exception ex)
        {

            throw ex;
        }
    }
    public async Task<bool> UpdateDecision(string Id, string action, string approvalId)
    {
        try
        {
            var rec = _context.Properties
                .FirstOrDefault(p => p.Id.ToString() == Id);
            if (rec == null)
                return false;
            if (action.ToLower() == "approved")
            {
                rec.ApprovalStatus = "Approved";
                rec.ActedOnBy = approvalId;

                //approve the sellerinfo if not approved 
                var getSeller = _context.SellerInfo.FirstOrDefault(p => p.UserId == rec.SellerId);
                var getUser = _context.Users.FirstOrDefault(o => o.Id == rec.SellerId);
                var names = getUser.FullName.Split(' ');

                var payload = new RootPayload
                {
                    Data = new Data
                    {
                        Ievent = new IEvent
                        {
                            Firstname = names[0],
                            Lastname = names[1],
                            Mobileno = rec.Seller.PhoneNumber,
                            Memberid = rec.SellerId,
                            Email = rec.Seller.Email,
                            Bvn = getSeller.Bvn,
                            Prefbank = "GTB",
                            Metadata = "nip"
                        }
                    }
                };

                var json = JsonSerializer.Serialize(payload);
                var account = await _apiCall.GenerateVirtualAccountAsync(json);
                if (getSeller.ApprovalStatus.ToLower() != "approved")
                {
                    getSeller.ApprovalStatus = "Approved";
                    getSeller.ActedOnBy = approvalId;
                    getSeller.VirtualAccount = account;
                }
                _context.Properties.Update(rec);

            }
            else
            {
                _context.Properties.Remove(rec);
            }
            await _context.SaveChangesAsync();
            return true;

        }
        catch (Exception ex)
        {
            return false;
            throw ex;
        }
    }


}