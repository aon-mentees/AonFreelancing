using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Models
{
    [Table("Otps")]
    public class Otp
    {
        [Key]
        public string PhoneNumber { get; set; }

        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public Otp() { }
        public Otp(string phoneNumber, string code, int expireInMinutes)
        {
            Code = code;
            PhoneNumber = phoneNumber;
            CreatedDate = DateTime.Now;
            ExpiresAt = DateTime.Now.AddMinutes(expireInMinutes);
        }

        public Otp(OtpInputDTO otpDTO)
        {
            Code = otpDTO.Code;
            PhoneNumber = otpDTO.PhoneNumber;
            CreatedDate = DateTime.Now;
            ExpiresAt = otpDTO.ExpiresAt;
        }
    }
}
