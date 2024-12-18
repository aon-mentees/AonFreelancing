using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Services
{
    public class ClientService : MainDbService
    {
        public ClientService(MainAppContext mainAppContext) : base(mainAppContext) { }

       
        public async Task<Client?> FindClientByIdAsync(long clientId)
        { 
            return await _mainAppContext.Users.OfType<Client>().AsNoTracking()
                .FirstOrDefaultAsync(c => c.Id == clientId && !c.IsDeleted);
        }
    }
}
