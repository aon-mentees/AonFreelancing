using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace AonFreelancing.Models;

[Table("TempUser")]
public class TempUser
{
    [Key]
    public long Id { get; set; }
    [Required]
    public string PhoneNumber { get; set; }
    public bool PhoneNumberConfirmed { get; set; }
    public TempUser() { }
    public TempUser(string phoneNumber)
    {
        PhoneNumber = phoneNumber;
    }

}