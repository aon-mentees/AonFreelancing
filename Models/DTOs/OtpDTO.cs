namespace AonFreelancing.Models.DTOs
{
    public class OtpInputDTO 
    {
        public string PhoneNumber { get; set; }
        public string Code { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; }
        public OtpInputDTO(string phoneNumber, string code, int expireInMinutes)
        {
            Code = code;
            PhoneNumber = phoneNumber;
            CreatedDate = DateTime.Now;
            ExpiresAt = DateTime.Now.AddMinutes(expireInMinutes);
        }
    }
}