using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.Requests
{
    public class PhoneVerificationRequest
    {
        [Required, StringLength(14, MinimumLength = 14)]
        public string Phone { get; set; }

        [Required, StringLength(6, MinimumLength = 6)]
        [JsonPropertyName("otp")]
        public string OtpCode { get; set; }

    }
}
