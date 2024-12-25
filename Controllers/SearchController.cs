using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mixvel.Interfaces;

namespace Mixvel.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ILogger<SearchController> _logger;
        private readonly ISearchService _searchService;
        public SearchController(ILogger<SearchController> logger, ISearchService searchService)
        {
            _logger = logger;
            _searchService = searchService;
        }

        [HttpGet("IsAvailable")]
        public Task<bool> IsAvailableAsync(CancellationToken token)
        {
            return _searchService.IsAvailableAsync(token);
        }

        [HttpPost]
        public Task<SearchResponse> Search([FromBody]SearchRequest request, CancellationToken cancellationToken)
        {
            return _searchService.SearchAsync(request, cancellationToken);
        }
    }
}
