using AonFreelancing.Models.DTOs;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models
{
    public class TokenBlacklist
    {
        
        [Key]
        public int Id { get; set; }
        [Required]
        public string Email { get; set; }
        [Required]
        public string TokenHash { get; set; } 
        [Required]
        public DateTime ExpiredAt { get; set; }
        [Required]
        public DateTime BlacklistedAt { get; set; }

       

    }
}
