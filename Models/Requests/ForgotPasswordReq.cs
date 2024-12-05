using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record ForgotPasswordReq(
        [Required, PhoneNumberRegex, StringLength(14, MinimumLength = 14)] 
        string? PhoneNumber
    );
}
