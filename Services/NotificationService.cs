using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Models.Responses;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class NotificationService
    {
        readonly MainAppContext _mainAppContext;

        public NotificationService(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        public async Task CreateAsync(Notification notification)
        {
            await _mainAppContext.Notifications.AddAsync(notification);
            await _mainAppContext.SaveChangesAsync();
        }

        public async Task DeleteForLikeAsync(ProjectLike projectLike)
        {
            LikeNotification? storedLikeNotification = await _mainAppContext.Notifications
                                                                            .OfType<LikeNotification>()
                                                                            .FirstOrDefaultAsync(ln => ln.LikerId == projectLike.LikerId);
            if (storedLikeNotification == null)
                return;
            _mainAppContext.Remove(storedLikeNotification);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task<List<Notification>> FindNotificationsForUserAsync(long userId)
        {
            return await _mainAppContext.Notifications.AsNoTracking()
                                                      .Where(n => n.ReceiverId == userId)
                                                      .ToListAsync();
        }
        public async Task MarkNotificationsAsReadAsync(List<Notification> notifications)
        {
            notifications.Where(n => !n.IsRead).ToList()
                                               .ForEach(n =>
            {
                n.IsRead = true;
                _mainAppContext.Notifications.Update(n);
            });
            await _mainAppContext.SaveChangesAsync();

        }
    }
}
