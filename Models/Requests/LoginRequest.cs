using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record LoginRequest(
        [Required, StringLength(14, ErrorMessage = "Invalid Phone Number, should be 14 digit starts with country code")] 
        string PhoneNumber,
        [Required, MinLength(4, ErrorMessage = "Invalid Password")] 
        string Password
    );
}
