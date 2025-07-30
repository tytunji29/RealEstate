namespace RealEstate.RequestDtos
{
    public class LoginFlowDto
    {
    }
    public class SellerDto
    {
        public string Id { get; set; }
        public string ApprovalStatus { get; set; }
    }
    public class RegisterDto
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        //public string Role { get; set; }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }
}
