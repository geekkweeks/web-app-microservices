using Microsoft.AspNetCore.Mvc;
using MongoDB.Entities;
using SearchService.Models;
using SearchService.RequestHelper;

namespace SearchService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        [HttpGet]
        public async Task<ActionResult<List<Item>>> SearchItems([FromQuery]SearchParams request)
        {
            var query = DB.PagedSearch<Item, Item>();

            query.Sort(x => x.Ascending(a => a.Make));

            if (!string.IsNullOrEmpty(request.SearchTerm))
            {
                query.Match(Search.Full, request.SearchTerm).SortByTextScore();
            }

            query = request.OrderBy switch
            {
                "make" => query.Sort(x => x.Ascending(a => a.Make)),
                "new" => query.Sort(x => x.Descending(a => a.CreatedAt)),
                _ => query.Sort(x => x.Ascending(a => a.AuctionEnd))
            };

            query = request.FilterBy switch
            {
                "finished" => query.Match(x => x.AuctionEnd < DateTime.UtcNow),
                "endingSoon" => query.Match(x => x.AuctionEnd < DateTime.UtcNow.AddHours(6)
                    && x.AuctionEnd > DateTime.UtcNow),
                _ => query.Match(x => x.AuctionEnd > DateTime.UtcNow)
            };

            if (!string.IsNullOrEmpty(request.Seller))
            {
                query.Match(x => x.Seller == request.Seller);
            }

            if (!string.IsNullOrEmpty(request.Winner))
            {
                query.Match(x => x.Winner == request.Winner);
            }

            query.PageNumber(request.PageNumber);
            query.PageSize(request.PageSize);

            var result = await query.ExecuteAsync();

            return Ok(new
            {
                data = result.Results,
                pageCount = result.PageCount,
                totalCount = result.TotalCount
            });
        }
    }
        
}
