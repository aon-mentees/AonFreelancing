using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AonFreelancing.Interfaces;
using AonFreelancing.Models.Requests;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Models
{
    //Replaced with 'builder.Entity<User>().HasIndex(u => u.PhoneNumber).IsUnique();' in OnModelCreate
    //[Index(nameof(PhoneNumber), IsUnique = true)]

    public class User : IdentityUser<long>
    {
        public string Name { get; set; }
        public string? About {  get; set; }

        public User() { }
        public User(UserRegistrationRequest request)
        {
            Name = request.Name;
            PhoneNumber = request.PhoneNumber;
            Email = request.Email;
        }
    }
}
