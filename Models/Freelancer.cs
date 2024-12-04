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
        public List<Skill> Skills { get; set; }

        public Freelancer() { }
        public Freelancer(UserRegistrationRequest registrationRequest)
        : base(registrationRequest)
        {

        }

    }
}
