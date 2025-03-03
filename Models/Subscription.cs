using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using AonFreelancing.Enums;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Models;

[Index(nameof(UserId))]
[Index(nameof(TransactionId))]

public class Subscription
{
    [Key]
    public string Id { get; set; } //same as order id
    public long UserId { get; set; }
    
    //int not decimal as  the only supported currency is IQD.
    public int Cost { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? ExpirationDateTime { get; set; }
    public string TransactionId { get; set; }
    public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
    
    public Subscription(){}

    public Subscription(string id, long userId, int cost, string transactionId)
    {
        Id = id;
        UserId = userId;
        Cost = cost;
        TransactionId = transactionId;
    }
}

