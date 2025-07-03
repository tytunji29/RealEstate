using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;
using RealEstate.Repository;

namespace RealEstate.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyRepository _propertyRepository;
    private readonly IUnitOfWork _unitOfWork;


    public PropertiesController(IPropertyRepository propertyRepository, IUnitOfWork unitOfWork)
    {
        _propertyRepository = propertyRepository;
        _unitOfWork = unitOfWork;
    }
    [Authorize]
    [HttpPost("CreateProperty")]
    public async Task<IActionResult> CreateProperty([FromForm] PropertyDto dto)
    {
        try
        {
            var sellerId = User.FindFirstValue("SellerId");
            if (string.IsNullOrEmpty(sellerId))
                return Unauthorized("SellerId not found in token.");
            // ✅ Null checks for files
            if (dto.DefaultImage == null)
                return BadRequest("Default image is required.");
            if (dto.ImageUrls == null || !dto.ImageUrls.Any())
                return BadRequest("At least one image is required.");

            //check if the seller as been validated
            var validated = await _unitOfWork.SellerInfos.FindAsync(o => o.UserId == sellerId);
            var rec = validated.FirstOrDefault();
            if (rec == null || rec.ApprovalStatus == "Pending")
            {
                var si = new SellerInfo
                {
                    Nin = dto.Nin,
                    Bvn = dto.Bvn,
                    UserId = sellerId,
                    ApprovalStatus = "Pending"
                };
                await _unitOfWork.SellerInfos.AddAsync(si);
                var re = new UnverifiedSellerProduct
                {
                    Data = JsonSerializer.Serialize(dto),
                    SellerId = sellerId
                };
                await _unitOfWork.UnverifiedSellerProducts.AddAsync(re);
                await _unitOfWork.CompleteAsync();

                return Ok(new ReturnObject { Data = null, Message = "Record Stored Pending Approval As This Is Your First Upload", Status = true });
            }

            // ✅ Map to entity
            var property = await _propertyRepository.PreparePropertyFromDtoAsync(dto, sellerId);
            // ✅ Save to DB
            await _propertyRepository.AddAsync(property);
            await _unitOfWork.CompleteAsync();
            return Ok(new ReturnObject { Data = null, Message = "Record Saved Successfully", Status = true });
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }



    [HttpGet("GetAllProperties")]
    public async Task<IActionResult> GetAllProperties()
    {
        var properties = await _propertyRepository.GetAllAsync();
        if (properties == null || !properties.Any())
            return NotFound("No properties found.");
        return Ok(properties.OrderByDescending(o => o.Id).ToList());
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPropertyById(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return NotFound();
        return Ok(property);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProperty(Guid id, [FromBody] PropertyDto dto)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return NotFound();

        property.Title = dto.Title;
        property.Description = dto.Description;
        property.Price = dto.Price;
        property.Location = dto.Location;
        property.PropertyType = dto.PropertyType;
        property.LandType = dto.LandType;
        property.BuildingType = dto.BuildingType;

        _propertyRepository.Update(property);
        await _unitOfWork.CompleteAsync();

        return Ok(property);
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProperty(Guid id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return NotFound();

        _propertyRepository.Remove(property);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
}