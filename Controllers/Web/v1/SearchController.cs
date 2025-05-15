using AonFreelancing.Contexts;
using AonFreelancing.Models;
using AonFreelancing.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AonFreelancing.Controllers.Web.v1;
[Controller]
    [Route("api/web/v1/search")]
public class SearchController: BaseController
{
    private readonly ElasticService<User> _elasticService;
private readonly MainAppContext _context;
    public SearchController(MainAppContext mainAppContext, ElasticService<User> elasticService)
    {
        _elasticService = elasticService;
        _context = mainAppContext;
    }

    [HttpPost("add")]
    public async Task<IActionResult> AddOrUpdateAsync(long id)
    {
        User? user =await _context.Users.FirstOrDefaultAsync(u =>u.Id == id);
        var result = await _elasticService.AddOrUpdate(user, id.ToString());
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Search(string query, int page = 0, int size = 10)
    {
        var result = await _elasticService.SearchAsync(query,page,size,["userName","name"]);
        return Ok(CreateSuccessResponse(result));
    }

    [HttpGet("automcomplete")]
    public async Task<IActionResult> Automcomplete(string prefix)
    {
        return Ok(CreateSuccessResponse(await _elasticService.AutocompleteAsync(prefix, "name")));
    }
}