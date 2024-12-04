using System.ComponentModel.DataAnnotations;
using AonFreelancing.Utilities;
namespace AonFreelancing.Models.Requests
{
    public record UserRegistrationRequest(
        [Required, MinLength(2)] 
        string Name,
        [Required, MinLength(4)]
        string Username,
        [Required, Phone] 
        string PhoneNumber,
        [Required, MinLength(6, ErrorMessage = "Too short password")]
        string Password,
        [Required, AllowedValues(Constants.USER_TYPE_FREELANCER, Constants.USER_TYPE_CLIENT)] 
        string UserType,
        string? CompanyName = null
    );
}
