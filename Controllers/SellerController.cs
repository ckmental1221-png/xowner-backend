using Microsoft.AspNetCore.Mvc;
using XownerWebOne.Data;
using XownerWebOne.DTOs;
using XownerWebOne.Models;

namespace XownerWebOne.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SellerController : ControllerBase
    {
        private readonly AppDbContext _context;

        public SellerController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public IActionResult Create(SellerCreateDto dto)
        {
            var seller = new Seller
            {
                Name = dto.Name,
                ShopName = dto.ShopName,
                Phone = dto.Phone
            };

            _context.Sellers.Add(seller);
            _context.SaveChanges();

            return Ok(seller);
        }
    }
}
