using AonFreelancing.Attributes;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests;

public class PhoneNumberReq
{
    [Required]
    [PhoneNumberRegex]
    public string PhoneNumber { get; set; }


}