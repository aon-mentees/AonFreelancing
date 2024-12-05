using System.ComponentModel.DataAnnotations;
using AonFreelancing.Utilities;
namespace AonFreelancing.Models.Requests
{
    public class UserRegistrationRequest
    {
        [Required, MinLength(2)]
        public string Name { get; set; }
        //[Required] 
        //[MinLength(4)]
        //string Username { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Too short password")]
        [RegularExpression("^(?=.*[a-z])(?=.*[A-Z])(?=.*\\d)(?=.*[#$^+=!*()@%&]).{6,}$", ErrorMessage = "Password must be at least 6 characters long, contains at least one uppercase letter, one lowercase letter, one number, and one special character (#$^+=!*()@%&).")]
        public string Password { get; set; }
        [Required, AllowedValues([Constants.USER_TYPE_FREELANCER, Constants.USER_TYPE_CLIENT], ErrorMessage = $"user type must be either '{Constants.USER_TYPE_FREELANCER}' or '{Constants.USER_TYPE_CLIENT}'")]
        public string UserType { get; set; }
        public string? CompanyName { get; set; }
    }
}
