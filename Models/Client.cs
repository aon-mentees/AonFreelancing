using AonFreelancing.Models.Requests;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AonFreelancing.Models
{

    [Table("Clients")]
    public class Client : User
    {
        public string CompanyName { get; set; }
        public List<Project>? Projects { get; set; }
        public Client() { }
        public Client(UserRegistrationRequest registrationRequest)
        : base(registrationRequest)
        {
            CompanyName = registrationRequest.CompanyName;
        }

    }
}
