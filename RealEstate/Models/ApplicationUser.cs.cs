using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace RealEstate.Models;


public class ApplicationUser : IdentityUser
{
    [Required] public string FullName { get; set; }
    public string Address { get; set; }
    public string PhoneNumber { get; set; }
    public string UserRole { get; set; }
    public string Status { get; set; }
    public DateTime CreatedDate { get; set; }
    public ICollection<Property> Properties { get; set; }
    public ICollection<PropertyViewers> PropertyViewers { get; set; }
}
