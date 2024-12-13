using AonFreelancing.Models.DTOs;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Certifications")]
    public class Certification
    {
        public long Id { get; set; }
        public Freelancer Freelancer { get; set; }
        public long FreelancerId { get; set; }
        public string Name { get; set; }
        public string Issuer { get; set; }
        public string? CredentialId { get; set; }
        public string? CredentialUrl { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public Certification() { }
        public Certification(CertificationInputDTO certificationInputDTO, long freelancerId)
        {
            FreelancerId = freelancerId;
            Name = certificationInputDTO.Name;
            Issuer = certificationInputDTO.Issuer;
            CredentialId = certificationInputDTO.CredentialId;
            CredentialUrl = certificationInputDTO.CredentialUrl;
            IssueDate = certificationInputDTO.IssueDate;
            ExpiryDate = certificationInputDTO.ExpiryDate;
        }
        public static Certification FromCertificationInputDTO(CertificationInputDTO certificationInputDTO, long freelancerId) 
            => new Certification(certificationInputDTO, freelancerId);

    }
}
