using Microsoft.AspNetCore.Http.HttpResults;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.DTOs
{
    public class EducationOutputDTO
    {
        public long Id { get; set; }
        public string Institution { get; set; }
        public string Degree { get; set; }
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }

        public EducationOutputDTO(Education education)
        {
            Id = education.Id;
            Institution = education.Institution;
            Degree = education.Degree;
            startDate = education.startDate;
            endDate = education.endDate;
        }
        public static EducationOutputDTO FromEducation(Education education) => new EducationOutputDTO(education);
    }
}
