using AonFreelancing.Contexts;
using AonFreelancing.Models.Documents;
using AonFreelancing.Services;
using Microsoft.AspNetCore.Mvc;

namespace AonFreelancing.Controllers.Mobile.v1;

[Controller]
[Route("api/mobile/v1/search")]
public class SearchController: BaseController
{
    private readonly ElasticService<UserDocument> _elasticService;
    private readonly MainAppContext _context;
    public SearchController(MainAppContext mainAppContext, ElasticService<UserDocument> elasticService)
    {
        _elasticService = elasticService;
        _context = mainAppContext;
    }
    
    [HttpGet]
    public async Task<IActionResult> Search(string query, int page = 0, int size = 10)
    {
        var result = await _elasticService.SearchAsync(query,page,size,["username","name"]);
        return Ok(CreateSuccessResponse(result));
    }

    [HttpGet("automcomplete")]
    public async Task<IActionResult> Automcomplete(string prefix)
    {
        return Ok(CreateSuccessResponse(await _elasticService.AutocompleteAsync(prefix, "name")));
    }
}