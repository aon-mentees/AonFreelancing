using System.Security.Claims;
using AonFreelancing.Commons;
using AonFreelancing.Enums;
using AonFreelancing.Models;
using AonFreelancing.Services;
using AonFreelancing.Utilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZainCash.Net.DTOs;
using ZainCash.Net.Services;
using ZainCash.Net.Utils;

namespace AonFreelancing.Controllers.Mobile.v1;

[Authorize]
[Route("api/mobile/v1/[controller]")]
public class PaymentsController : BaseController
{
    private readonly ZainCashService _zainCashService;
    private readonly SubscriptionsService _subscriptionsService;
    private readonly IConfiguration _configuration;
    private readonly ZainCashAPIConfig _zainCashApiConfig;
    private readonly AuthService _authService;

    public PaymentsController(ZainCashService zainCashService, SubscriptionsService subscriptionsService,
        IConfiguration configuration, ZainCashAPIConfig zainCashApiConfig, AuthService authService)
    {
        _zainCashService = zainCashService;
        _subscriptionsService = subscriptionsService;
        _configuration = configuration;
        _zainCashApiConfig = zainCashApiConfig;
        _authService = authService;
    }

    [HttpPost]
    public async Task<IActionResult> InitTransaction()
    {
        long userId = _authService.GetUserId((ClaimsIdentity)User.Identity);
        string orderId = Guid.NewGuid().ToString();
        int subscriptionCost = _configuration.GetValue<int>("Subscription:Cost");


        InitTransactionRequest request = new InitTransactionRequest()
        {
            Amount = subscriptionCost,
            OrderId = orderId,
        };
        InitTransactionResponse response = await _zainCashService.InitTransactionAsync(request);
        Subscription newSubscription = new Subscription(orderId, userId, subscriptionCost, response.Id);
        await _subscriptionsService.SaveAsync(newSubscription);
        return Ok(CreateSuccessResponse(response.Url));
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> ActivateSubscription(string token)
    {
        // TokenResult decodedToken = _zainCashService.DecodeToken(token);
        CustomTokenResult decodedToken = CustomZainCashUtil.DecodeToken(token, _zainCashApiConfig.Secret);

        Subscription? storedSubscription = await _subscriptionsService.FindByIdAsync(decodedToken.OrderId);
        if (storedSubscription == null)
            return NotFound(CreateErrorResponse(StatusCodes.Status404NotFound.ToString(), "subscription not found"));

        if (!decodedToken.Status.Equals("completed"))
        {
            storedSubscription.Status = SubscriptionStatus.Failed;
            await _subscriptionsService.UpdateAsync(storedSubscription);
            return StatusCode(StatusCodes.Status402PaymentRequired,CreateErrorResponse(StatusCodes.Status402PaymentRequired.ToString(),
                $"payment status: {decodedToken.Status} : {decodedToken.Msg}"));
        }

        int subscriptionDurationInDays = _configuration.GetValue<int>("Subscription:DurationInDays");
        storedSubscription.Status = SubscriptionStatus.Active;
        storedSubscription.StartDateTime = DateTime.Now;
        storedSubscription.ExpirationDateTime =
            storedSubscription.StartDateTime.Value.AddDays(subscriptionDurationInDays);

        await _subscriptionsService.UpdateAsync(storedSubscription);

        return Ok(CreateSuccessResponse("Subscription activated"));
    }
}