
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RealEstate.Helpers;
using RealEstate.Models;
using RealEstate.Models.ResponseDto;
using RealEstate.Repository;
using RealEstate.RequestDtos;

namespace RealEstate.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPropertyRepository _propertyRepository;

        public UsersController(UserManager<ApplicationUser> userManager, IPropertyRepository propertyRepository, IUnitOfWork unitOfWork, SignInManager<ApplicationUser> signInManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _propertyRepository = propertyRepository;
            _unitOfWork = unitOfWork;
            _signInManager = signInManager;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var userChecker = await _userManager.FindByEmailAsync(model.Email);
            if (userChecker is not null)
                return BadRequest(new ReturnObject { Message = "Email Already Exist.", Status = false });
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                UserRole = "customer", //by defult
                Address = model.Address,
                Status = "Active",
                CreatedDate = DateTime.Now
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded) return BadRequest(new ReturnObject { Message = "An Error Occured.", Status = false });

            return Ok(new ReturnObject { Message = "Registration successful.", Status = true });
        }

        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpPost("adminapprove")]
        public async Task<IActionResult> AdminApprove(SellerDto sellerDto)
        {
            var ret = new ReturnObject();
            var sellerId = User.FindFirstValue("SellerId");
            var role = User.FindFirstValue("Role");
            if (role.ToLower() != "admin")
                return BadRequest(new ReturnObject { Message = "Invalid Role", Status = false });

            var validated = await _unitOfWork.UnverifiedSellerProducts.FindAsync(o => o.Id.ToString() == sellerDto.Id);
            if (validated == null)
                return BadRequest(new ReturnObject { Message = "Invalid Id", Status = false });

            var rec = validated.FirstOrDefault();
            // Guid sellerIdx = Guid.Parse(rec.SellerId);
            var sellInfo = await _unitOfWork.SellerInfos.FindAsync(o => o.UserId == rec.SellerId);
            SellerInfo? sl = sellInfo.FirstOrDefault();
            if (sellerDto.ApprovalStatus.ToLower() == "rejected")
            {
                _unitOfWork.SellerInfos.Remove(sl);
                _unitOfWork.UnverifiedSellerProducts.Remove(rec);

                ret = new ReturnObject { Message = "Rejected successful.", Status = true };
            }
            else
            {
                sl.ActedOnBy = sellerId;
                sl.UpdatedAt = DateTime.Now;
                sl.ApprovalStatus = sellerDto.ApprovalStatus;
                _unitOfWork.SellerInfos.Update(sl);

                var dto = JsonSerializer.Deserialize<PropertyDtoII>(rec.Data);
                var property = await _propertyRepository.PreparePropertyFromDtoAsync(dto, sellerId);
                // ✅ Save to DB
                await _propertyRepository.AddAsync(property);

                ret = new ReturnObject { Message = "Approval successful.", Status = true };
            }
            await _unitOfWork.CompleteAsync();
            return Ok(ret);
        }
        [HttpPost("changeuserstatus")]
        public async Task<IActionResult> ChangeUserStatus(SellerDto sellerDto)
        {
            var ret = new ReturnObject();
            ret.Status = true;
            ret.Message = "Record Updated Successfully";
            
            var user = await _unitOfWork.Users.GetByIdAsync(sellerDto.Id);
            if (user == null)
                return BadRequest(new ReturnObject { Message = "Invalid Id", Status = false });

            if (sellerDto.ApprovalStatus.ToLower() == "rejected")
            {
                user.Status = "Rejected";
            }
            switch (sellerDto.ApprovalStatus.ToLower())
            {

                case "active":
                    user.Status = "Active";
                    break;
                case "rejected":
                    user.Status = "Inactive";
                    break; 
                case "admin":
                    user.UserRole = "admin";
                    break;
                default:
                    break;
            }
            _unitOfWork.Users.Update(user);
            await _unitOfWork.CompleteAsync();
            return Ok(ret);
        }

        [HttpGet("AllCustomerUsers")]
        public async Task<IActionResult> AllCustomerUsers(int pgNo = 1, int pgSize = 10, string? search = "")
        {
            var user = await _unitOfWork.Users.GetAllAsync();
            //select few entities 
            //add search filter 
            if (!string.IsNullOrEmpty(search))
            {
                user = user.Where(u => u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                      u.FullName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                                      u.PhoneNumber.Contains(search, StringComparison.OrdinalIgnoreCase));
            }
            var totalRecords = user.Count();
            user = user.OrderByDescending(u => u.CreatedDate)
                    .Skip((pgNo - 1) * pgSize)
                    .Take(pgSize)
                    .ToList();
            //pagination logic

            var users = user.Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                u.PhoneNumber,
                u.Status,
                u.UserRole,
                u.Address,
                u.CreatedDate
            }).ToList();

            return Ok(new ReturnObject { Message = "Record Found Successfully", Status = true, Data = new { users = users, totalRecords = totalRecords } });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var valid=await _unitOfWork.Users.FindAsync(o => o.Email == model.Email);
            if (valid == null || !valid.Any())
                return Unauthorized(new ReturnObject { Message = "Invalid Email or Password", Status = false });
            if(valid.FirstOrDefault().Status.ToLower() != "active")
                return Unauthorized(new ReturnObject { Message = "Your Account is not active, please contact support.", Status = false });
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized();

            var claims = new[]
            {
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Sub, user.Id),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Email, user.Email),
            new Claim("SellerId", user.Id),
            new Claim("Role", user.UserRole),
            new Claim(Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["appSettings:JwtKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new System.IdentityModel.Tokens.Jwt.JwtSecurityToken(
                _configuration["appSettings:EfcKey"],
                _configuration["appSettings:JwtAudience"],
                claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: creds
            );
            var propertyTypes = Enum.GetValues(typeof(PropertyType))
                                     .Cast<PropertyType>()
                                     .Select(e => new { Id = (int)e, Name = e.ToString() })
                                     .ToList();
            var LandType = Enum.GetValues(typeof(LandType))
                                     .Cast<LandType>()
                                     .Select(e => new { Id = (int)e, Name = e.ToString() })
                                     .ToList();
            var BuildingType = Enum.GetValues(typeof(BuildingType))
                                     .Cast<BuildingType>()
                                     .Select(e => new { Id = (int)e, Name = e.ToString() })
                                     .ToList();

            return Ok(new ReturnObject { Message = $"Welcome {user.Email}", Status = true, Data = new { BuildingType, LandType, propertyTypes, token = new JwtSecurityTokenHandler().WriteToken(token), role = user.UserRole, email = user.Email } });
        }
    }
}
