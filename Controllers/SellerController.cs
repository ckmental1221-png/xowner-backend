using Microsoft.AspNetCore.Mvc;
using Xowner.Com.DTOs;
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
                Phone = dto.Phone,
                JoinDate = DateTime.UtcNow   // 🔥 VERY IMPORTANT FOR RENDER
            };

            _context.Sellers.Add(seller);
            _context.SaveChanges();

            return Ok(seller);
        }

        // ================= GET SELLER BY ID =================
        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var seller = _context.Sellers.Find(id);

            if (seller == null)
                return NotFound(new { message = "Seller not found" });

            var result = new SellerReadDto
            {
                Id = seller.Id,
                Name = seller.Name,
                ShopName = seller.ShopName,
                Phone = seller.Phone,
                JoinDate = seller.JoinDate
            };

            return Ok(result);
        }

    }
}
