using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Models.Requests;

public class PhoneNumberReq
{
    [Required, StringLength(15, MinimumLength = 10)]
    [Phone]
    public string PhoneNumber { get; set; }


}