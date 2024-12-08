using System.Text.Json.Serialization;

namespace AonFreelancing.Models.DTOs
{
    public class OAuthUserInfoDTO
    {
        public string FirstName {  get; set; }
        public string LastName { get; set; }
        public string FullName {  get; set; }
        public string Email { get; set; }

        public OAuthUserInfoDTO(string firstName, string lastName, string email)
        {
            FirstName = firstName;
            LastName = lastName;
            FullName = $"{FirstName} {LastName}";
            Email = email;
        }
        //public string SimplePhoneNumber {  get; set; }
        //public string CanonicalPhoneNumber {  get; set; }


    }
}
