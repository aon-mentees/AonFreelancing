using AonFreelancing.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{
    [Table("Freelancers")]
    public class Freelancer : User
    {
        public List<Project> Projects { get; set; }
        public string? QualificationName { get; set; }
        public List<Skill> Skills { get; set; }
        public List<Certification> Certifications { get; set; }
        public List<Education> Education { get; set; }
        public List<WorkExperience> WorkExperiences { get; set; }
        public Freelancer() { }
        public Freelancer(UserRegistrationRequest registrationRequest)
        : base(registrationRequest)
        {
        }
    }
}
