using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class LikeNotificationService
    {
        readonly MainAppContext _mainAppContext;

        public LikeNotificationService(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        public async Task CreateAsync(LikeNotification likeNotification)
        {
            await _mainAppContext.LikeNotifications.AddAsync(likeNotification);
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task DeleteWithrojectLikeAsync(ProjectLike projectLike)
        {
            LikeNotification? storedLikeNotification = await _mainAppContext.LikeNotifications.FirstOrDefaultAsync(ln => ln.LikeId == projectLike.Id);
            if (storedLikeNotification == null)
                return;
            _mainAppContext.Remove(storedLikeNotification);
            await _mainAppContext.SaveChangesAsync();

        }
    }
}
