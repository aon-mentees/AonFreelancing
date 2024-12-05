using Microsoft.Extensions.FileSystemGlobbing.Internal;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace AonFreelancing.Attributes
{
    public class PhoneNumberRegexAttribute : ValidationAttribute
    {
        private readonly string pattern = @"^\+([1-9][0-9]{0,2})([0-9]{7,15})$";

        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                var phoneNumber = value.ToString();
                if (Regex.IsMatch(phoneNumber, pattern))
                    return ValidationResult.Success;
            }

            return new ValidationResult("This phone number is invalid !");

        }
    }
}
