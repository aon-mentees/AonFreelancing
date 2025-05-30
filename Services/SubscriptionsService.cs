using AonFreelancing.Contexts;
using AonFreelancing.Enums;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services;

public class SubscriptionsService
{
    private readonly MainAppContext _mainAppContext;

    public SubscriptionsService(MainAppContext mainAppContext)
    {
        _mainAppContext = mainAppContext;
    }
    public async Task<Subscription?> FindByIdAsync(string id)
    {
        return await _mainAppContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<List<Subscription>> FindAllByUserIdAsync(long userId)
    {
        return await _mainAppContext.Subscriptions.Where(s => s.UserId == userId).ToListAsync();
    }
    public async Task<List<Subscription>> FindAllActiveByUserIdAsync(long userId)
    {
        return await _mainAppContext.Subscriptions
            .Where(s => s.UserId == userId && s.Status == SubscriptionStatus.Active).ToListAsync();
    }

    public async Task<Subscription?> FindByTransactionIdAsync(string transactionId)
    {
        return await _mainAppContext.Subscriptions.FirstOrDefaultAsync(s => s.TransactionId == transactionId);
    }

    public async Task SaveAsync(Subscription newSubscription)
    {
        await _mainAppContext.AddAsync(newSubscription);
        await _mainAppContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(Subscription storedSubscription)
    {
        _mainAppContext.Update(storedSubscription);
        await _mainAppContext.SaveChangesAsync();
    }
}