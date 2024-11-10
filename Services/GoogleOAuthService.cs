using AonFreelancing.Models.DTOs;
using AonFreelancing.Models.Requests;
using System.Text.Json;

namespace AonFreelancing.Services
{
    public class GoogleOAuthService
    {
        readonly IConfiguration _configuration;
        readonly string clientId;
        readonly string clientSecret;
        readonly string redirectUri;
        readonly string tokenUrl;
        readonly string accessType;
        readonly string grantType;
        readonly Dictionary<string, string> tokenRequestParameters;
        public GoogleOAuthService(IConfiguration configuration)
        {
            _configuration = configuration;

            clientId = _configuration["oauth2:google:client_id"];
            clientSecret = _configuration["oauth2:google:client_secret"];
            tokenUrl = _configuration["oauth2:google:token_uri"];
            redirectUri = _configuration.GetSection("oauth2:google:redirect_uris").Get<string[]>()[0];
            accessType = _configuration["oauth2:google:access_type"];
            grantType = _configuration["oauth2:google:grant_type"];

            tokenRequestParameters = new Dictionary<string, string>()
                                      {
                                       {"client_id",clientId},
                                       {"client_secret",clientSecret},
                                       {"grant_type",grantType},
                                       {"redirect_uri",redirectUri},
                                       {"access_type",accessType}
                                      };
        }
        public async Task<OauthTokenResponse> RetrieveOAuthTokenAsync(string code)
        {
            var body = new Dictionary<string, string>(tokenRequestParameters) { { "code", code } };

            using (var client = new HttpClient())
            using (var content = new FormUrlEncodedContent(body))
            {
                HttpResponseMessage response = await client.PostAsync(tokenUrl, content);
                string jsonResponse = await response.Content.ReadAsStringAsync();

                return JsonSerializer.Deserialize<OauthTokenResponse>(jsonResponse);
            }
        }
        public async Task<OAuthUserInfoDTO> RetreiveUserInfo(string accessToken,string requestedFields)
        {
            //specify which info you need using personFields which is required
            string url = $"https://people.googleapis.com/v1/people/me?personFields={requestedFields}&oauth_token={accessToken}";
            using (var client = new HttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(url);
                var userInfoJsonResponse = await response.Content.ReadAsStringAsync();

                using (JsonDocument doc = JsonDocument.Parse(userInfoJsonResponse))
                {
                    var root = doc.RootElement;
                    var names = root.GetProperty("names")[0];
                    var familyName = names.GetProperty("familyName").GetString();
                    var givenName = names.GetProperty("givenName").GetString();

                    root.TryGetProperty("emailAddresses",out JsonElement emails);
                    emails[0].TryGetProperty("value",out JsonElement email);

                    root.TryGetProperty("phoneNumbers", out JsonElement phoneNumbers);
                    phoneNumbers[0].TryGetProperty("value",out JsonElement simplePhoneNumber);
                    phoneNumbers[0].TryGetProperty("canonicalForm", out JsonElement canonicalPhoneNumber);

                    return new OAuthUserInfoDTO
                    {
                        Email = email.GetString(),
                        FirstName = givenName,
                        LastName = familyName,
                        SimplePhoneNumber = simplePhoneNumber.GetString(),
                        CanonicalPhoneNumber = canonicalPhoneNumber.GetString()
                    };
                }
              
                //return  JsonSerializer.Deserialize<OAuthUserInfoDTO>(userInfoJsonResponse);
            }
        }
    }
}
