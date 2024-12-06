using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests
{
    public record ForgotPasswordReq(
        [Required, PhoneNumber, StringLength(14, MinimumLength = 14)] 
        string? PhoneNumber
    );
}
