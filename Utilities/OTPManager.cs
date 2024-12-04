using Twilio.Types;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using AonFreelancing.Models;

namespace AonFreelancing.Utilities
{
    public class OtpManager
    {
      

        readonly IConfiguration _configuration;

        public OtpManager(IConfiguration configuration)
        {
            _configuration = configuration;
        }
        //TODO: make this method void
        public async Task SendOTPAsync(string otp,string receiverPhoneNumber)
        {
            if (_configuration["Env"] == Constants.ENV_SIT)
                return;
            var accountSid = _configuration["Twilio:Sid"];
            var authToken = _configuration["Twilio:Token"];
            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{receiverPhoneNumber}")); //To
            messageOptions.From = new PhoneNumber(_configuration["Twilio:From"]);//TODO use appsetting.json
            messageOptions.ContentSid = _configuration["Twilio:ContentSid"];
            messageOptions.ContentVariables = "{\"1\":\"" + otp + "\"}";


            var message =await MessageResource.CreateAsync(messageOptions);
        }
        public async Task SendForgotPasswordMessageAsync(string message,string receiverPhoneNumber)
        {
            if (_configuration["Env"] == Constants.ENV_SIT)
            {
                return;
            }

            var accountSid = _configuration["Twilio:Sid"];
            var authToken = _configuration["Twilio:Token"];
            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(new PhoneNumber($"whatsapp:{receiverPhoneNumber}")); 
            messageOptions.From = new PhoneNumber(_configuration["Twilio:From"]);
            messageOptions.ContentSid = _configuration["Twilio:ContentSid"];
            messageOptions.ContentVariables = "{\"1\":\"" + message + "\"}";

            var finalMessage = await MessageResource.CreateAsync(messageOptions);
        }
        public string GenerateOtp()
        {
            var otp = 123456;
            var random = new Random();
            if (_configuration["Env"] == Constants.ENV_SIT)
            {
                return otp.ToString("D6");
            }
            // Generate a random number between 0 and 999999
            otp = random.Next(0, (int)Math.Pow(10, 6));

            // Format the number to ensure it's always 6 digits
            return otp.ToString("D6");
        }
    }
}
