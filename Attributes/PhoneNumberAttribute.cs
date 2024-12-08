using PhoneNumbers;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Attributes
{
    public class PhoneNumberAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value is not string phoneNumber)
                return new ValidationResult("Invalid phone number format !");

            var phoneNumberUtil = PhoneNumberUtil.GetInstance();

            try
            {
                var parsedNumber = phoneNumberUtil.Parse(phoneNumber, null); // can add region instead of null
                if (!phoneNumberUtil.IsValidNumber(parsedNumber))
                    return new ValidationResult("The Phone number is invalid !");
            }
            catch (NumberParseException)
            {
                return new ValidationResult("The Phone number is invalid !");
            }

            return ValidationResult.Success;
        }
    }
}
