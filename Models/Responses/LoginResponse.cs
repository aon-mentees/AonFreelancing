using AonFreelancing.Models.DTOs;
using System.Data;
using Twilio.Jwt.AccessToken;

namespace AonFreelancing.Models.Responses
{
    public class LoginResponse
    {
        public UserDetailsDTO UserDetailsDTO { get; set; }
        public string AccessToken { get; set; }
        
        public LoginResponse() { }  
        public LoginResponse(string accessToken, UserDetailsDTO userDetailsDTO)
        {
            AccessToken = accessToken;
            UserDetailsDTO = userDetailsDTO;
        }
    }
}
