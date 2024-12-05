using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record LoginRequest(
        [Required]
        [PhoneNumberRegex]
        string PhoneNumber,
        [Required, MinLength(4, ErrorMessage = "Invalid Password")] 
        string Password
    );
}
