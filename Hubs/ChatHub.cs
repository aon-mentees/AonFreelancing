using AonFreelancing.Models.DTOs;
using AonFreelancing.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;

namespace AonFreelancing.Hubs
{
    public class ChatHub:Hub
    { 
         readonly ILogger<ChatHub> _logger;
        readonly InMemoryUserConnectionService _inMemoryUserConnectionService;
        public ChatHub(ILogger <ChatHub>logger,InMemoryUserConnectionService inMemoryUserConnectionService)
        {
            _logger = logger;
            _inMemoryUserConnectionService = inMemoryUserConnectionService;
        }
        public override async Task OnConnectedAsync()
        {
            string userId = Context.GetHttpContext().Request.Query["userId"];
            _inMemoryUserConnectionService.Add(userId,Context.ConnectionId);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            string userId = Context.GetHttpContext().Request.Query["userId"];
            _inMemoryUserConnectionService.Remove(userId, Context.ConnectionId);

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendChatMessage(ChatMessageDTO chatMessageDTO)
        { 
            var validationResults = new List<ValidationResult>();
            bool isValid = Validator.TryValidateObject(chatMessageDTO, new ValidationContext(chatMessageDTO), validationResults, true);
            if (!isValid)
            {
                foreach (var validationResult in validationResults)
                    _logger.LogError(validationResult.ErrorMessage);

                return; 
            }

            var receivers = _inMemoryUserConnectionService.GetConnections(chatMessageDTO.receiver);
            if (receivers == null)
                return;
            await Clients.Users(receivers).SendAsync("ReceiveMessage", chatMessageDTO);
        }


    }
}
