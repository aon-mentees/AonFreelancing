using AonFreelancing.Models.DTOs;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AonFreelancing.Models
{
    [Table("Education")]
    public class Education
    {
        public long Id { get; set; }
        public long freelancerId { get; set; }
        public Freelancer Freelancer { get; set; }
        public string Institution { get; set; }
        public string Degree { get; set; }

        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        public Education() { }
        Education(EducationInputDTO inputDTO, long id)
        {
            Institution = inputDTO.Institution;
            Degree = inputDTO.Degree;
            startDate = inputDTO.StartDate;
            endDate = inputDTO.EndDate;
            freelancerId = id;
        }
        public static Education FromEducationInputDTO(EducationInputDTO inputDTO, long freelancerId) => new Education(inputDTO, freelancerId);
    }
}
