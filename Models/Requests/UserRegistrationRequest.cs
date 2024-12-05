using System.ComponentModel.DataAnnotations;
using AonFreelancing.Attributes;
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
        [PhoneNumberRegex]
        public string PhoneNumber { get; set; }
        [Required]
        [MinLength(6, ErrorMessage = "Too short password")]
        public string Password { get; set; }
        [Required, AllowedValues(Constants.USER_TYPE_FREELANCER, Constants.USER_TYPE_CLIENT)]
        public string UserType { get; set; }
        public string? CompanyName { get; set; }
    }
}
