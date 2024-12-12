using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class CertificationInputDTO
    {
        [Required]
        [MaxLength(256, ErrorMessage = "Name is too long.")]
        public string Name { get; set; }
        [Required]
        [MaxLength(256, ErrorMessage = "Issuer is too long.")]
        public string Issuer { get; set; }
        public string? CredentialId { get; set; }
        [Url]
        public string? CredentialUrl { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
    }
}
