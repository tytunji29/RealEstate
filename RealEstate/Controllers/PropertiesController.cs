using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;
using RealEstate.Repository;

namespace RealEstate.Controllers;
//[Authorize]
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
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
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
                    ApprovalStatus = "Pending",
                    VirtualAccount="",
                    ActedOnBy = ""
                };
                await _unitOfWork.SellerInfos.AddAsync(si);
            }

            var property = await _propertyRepository.PrepareAndSavePropertyAsync(dto, sellerId);
            await _unitOfWork.CompleteAsync();

            return Ok(new ReturnObject { Data = null, Message = "Record Stored Pending Approval As This Is Your First Upload", Status = true });
        }
        catch (Exception ex)
        {

            throw ex;
        }
    }
    [HttpGet("GetAllProperties")]
    public async Task<IActionResult> GetAllProperties(int pgNo = 1, int pgSize = 10)
    {
        var properties = await _propertyRepository.GetAllPropertiesAsync(pgNo,pgSize,"Approved");
        if (properties.Item1 == null || !properties.Item1.Any())
            return Ok(new ReturnObject { Data = null, Message = "No properties found.", Status = false });
        var rec= properties.Item1.OrderByDescending(o => o.Id).ToList();

        return Ok(new ReturnObject { Data = new { record = rec, totalCount = properties.Item2 }, Message = "Record Found Successfully", Status = true });
    }
    [HttpGet("GetAllPendingProperties")]
    public async Task<IActionResult> GetAllPendingProperties(int pgNo = 1, int pgSize = 10)
    {
        var properties = await _propertyRepository.GetAllPropertiesAsync(pgNo, pgSize, "pending");
        if (properties.Item1 == null || !properties.Item1.Any())
            return Ok(new ReturnObject { Data = null, Message ="No properties found.", Status = false });
        var rec = properties.Item1.OrderByDescending(o => o.Id).ToList();

        return Ok(new ReturnObject { Data = new { record = rec, totalCount = properties.Item2 }, Message = "Record Found Successfully", Status = true });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPropertyById(string id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return NotFound();
        return Ok(property);
    }

    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProperty(string id, [FromBody] PropertyDto dto)
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
    [HttpPost("DecidePropertyById/{id}")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public async Task<IActionResult> DecideProperty(string id, [FromBody] PropertyDecisionDto dto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var property = await _propertyRepository.UpdateDecision(id, dto.Status, userId);
        if (@property)
            return Ok(new ReturnObject { Data = null, Message = "No properties found with this Id", Status = false });
        await _unitOfWork.CompleteAsync();

        return Ok(new ReturnObject { Data = null, Message = "Record Acted On Successfully", Status = true });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProperty(string id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);
        if (property == null) return NotFound();

        _propertyRepository.Remove(property);
        await _unitOfWork.CompleteAsync();

        return NoContent();
    }
}