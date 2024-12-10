using AonFreelancing.Models.DTOs;
using System.Text.Json.Serialization;

namespace AonFreelancing.Models.Responses
{
    public class LoginResponse
    {
        [JsonPropertyName("userDetails")]
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
