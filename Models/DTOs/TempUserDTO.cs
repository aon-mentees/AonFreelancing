using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class TempUserDTO
    {
        [Required, StringLength(14, MinimumLength = 14)]
        [Phone]
        public string PhoneNumber { get; set; }
    }
}