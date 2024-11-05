using AuctionService.Data;
using AuctionService.Dtos;
using AuctionService.Entities;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuctionService.Controllers
{
    [ApiController]
    [Route("api/auctions")]
    public class AuctionController : Controller
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AuctionController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<List<AuctionDto>>> GetAuctions()
        {
            var auctions = await _context.Auctions
                .Include(x => x.Item)
                .OrderBy(x => x.Item.Make)
                .ToListAsync();

            return _mapper.Map<List<AuctionDto>>(auctions);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AuctionDto>> GetAuctionById(Guid id)
        {
            var auction = await _context.Auctions
                .Include(x => x.Item)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (auction == null) return NotFound();

            return _mapper.Map<AuctionDto>(auction);
        }

        [HttpPost]
        public async Task<ActionResult<AuctionDto>> CreateAuction(CreateAuctionDto request)
        {
            var auction = _mapper.Map<Auction>(request);
            // TODO: add current user as seller
            auction.Seller = "system";

            _context.Auctions.Add(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Could not save changes to the DB");

            return CreatedAtAction(nameof(GetAuctionById), new { auction.Id }, _mapper.Map<AuctionDto>(auction));

        }

        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateAuction(Guid id, UpdateAuctionDto request)
        {
            Console.WriteLine(id);
            var auction = await _context.Auctions.Include(x => x.Item)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            auction.Item.Make = request.Make ?? auction.Item.Make;
            auction.Item.Model = request.Model ?? auction.Item.Model;
            auction.Item.Color = request.Color ?? auction.Item.Color;
            auction.Item.Mileage = request.Mileage ?? auction.Item.Mileage;
            auction.Item.Year = request.Year ?? auction.Item.Year;

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Problem saving changes");

            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteAuction(Guid id)
        {
            var auction = await _context.Auctions.FindAsync(id);

            if (auction == null) return NotFound();

            // TODO: check seller == username

            _context.Auctions.Remove(auction);

            var result = await _context.SaveChangesAsync() > 0;

            if (!result) return BadRequest("Problem deleting the auction");

            return Ok();
        }

    }

}