using AonFreelancing.Contexts;
namespace AonFreelancing.Services
{
    public class MainDbService
    {
        protected readonly MainAppContext _mainAppContext;
        public MainDbService(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }
                
        public async Task AddAsync(object obj)
        {
            await _mainAppContext.AddAsync(obj);
        }
        public void Remove(object obj)
        {
            _mainAppContext.Remove(obj);
        }
        public async Task SaveChangesAsync()
        {
            await _mainAppContext.SaveChangesAsync();
        }
        public async Task DeleteAsync(object obj)
        {
            _mainAppContext.Remove(obj);
            await _mainAppContext.SaveChangesAsync();
        }
    }
}