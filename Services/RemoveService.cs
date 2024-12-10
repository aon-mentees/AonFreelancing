using AonFreelancing.Contexts;

namespace AonFreelancing.Services;
public class RemoveService(MainAppContext mainAppContext)
{
    private readonly MainAppContext _mainAppContext = mainAppContext;
    public async Task RemoveEntityAsync(object entity)
    {
        _mainAppContext.Remove(entity);
        await _mainAppContext.SaveChangesAsync();
    }
}