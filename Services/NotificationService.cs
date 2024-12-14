using AonFreelancing.Contexts;
using AonFreelancing.Models;
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

        //public async Task DeleteWithrojectLikeAsync(ProjectLike projectLike)
        //{
        //    LikeNotification? storedLikeNotification = await _mainAppContext.Notifications.FirstOrDefaultAsync(n => n. == projectLike.Id);
        //    if (storedLikeNotification == null)
        //        return;
        //    _mainAppContext.Remove(storedLikeNotification);
        //    await _mainAppContext.SaveChangesAsync();

        //}
    }
}
