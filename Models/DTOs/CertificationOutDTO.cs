namespace AonFreelancing.Models.DTOs
{
    public class CertificationOutDTO
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public string Issuer { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? ExpiryDate { get; set; }

        public CertificationOutDTO(Certification certification)
        {
            Id = certification.Id;
            Name = certification.Name;
            Issuer = certification.Issuer;
            IssueDate = certification.IssueDate;
            ExpiryDate = certification.ExpiryDate;
        }

        public static CertificationOutDTO FromCertification(Certification certification) => new CertificationOutDTO(certification);
    }
}
