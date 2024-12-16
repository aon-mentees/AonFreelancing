using AonFreelancing.Contexts;
using AonFreelancing.Models.Responses;
using AonFreelancing.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using AonFreelancing.Models.DTOs.NoftificationDTOs;
using AonFreelancing.Utilities;

namespace AonFreelancing.Services
{
    public class ProfileService(MainAppContext mainAppContext, PushNotificationService pushNotificationService, UserService userService)
    {
        public async Task<Client?> FindClientAsync(long clientId)
        {
            return await mainAppContext.Users.OfType<Client>().FirstOrDefaultAsync(c => c.Id == clientId);
        }
        public async Task<PaginatedResult<Project>> FindClientActivitiesAsync(long clientId, int pageNumber, int pageSize)
        {
            var query = mainAppContext.Projects.AsNoTracking().Where(p => p.ClientId == clientId)
                                                              .Include(p => p.ProjectLikes)
                                                              .Include(p => p.Comments)
                                                              .AsQueryable();

            List<Project>? storedProjects = await query.OrderByDescending(p => p.CreatedAt)
                                                       .Skip(pageNumber * pageSize)
                                                       .Take(pageSize)
                                                       .ToListAsync();

            return new PaginatedResult<Project>(await query.CountAsync(), storedProjects);
        }
        public async Task VisitProfileAsync(long visitorId, long receiverId)
        {
            var visitorName = await userService.FindByIdAsync(visitorId);

            var profileVisitNotification = new ProfileVisitNotification(
                Constants.PROFILE_VISIT_NOTIFICATION_TITLE,
                string.Format(Constants.PROFILE_VISIT_NOTIFICATION_MESSAGE_FORMAT, visitorName),
                receiverId,
                string.Empty, // low level code --> for lilo 
                visitorId,
                visitorName.Name

            );

            var notificationDTO = new ProfileVisitNotificationOutputDTO(profileVisitNotification);
            await pushNotificationService.SendProfileVisitNotification(notificationDTO, receiverId);
        }
    }
}