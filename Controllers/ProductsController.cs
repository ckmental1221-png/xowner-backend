using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XownerWebOne.Data;
using XownerWebOne.DTOs;
using XownerWebOne.Models;

namespace XownerWebOne.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ================= CREATE PRODUCT =================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized("Login required");

            if (!int.TryParse(userId, out int sellerId))
                return BadRequest("Invalid user id");

            var product = new Product
            {
                Title = dto.Title,
                Category = dto.Category,
                Brand = dto.Brand,
                Model = dto.Model,
                Condition = dto.Condition,
                Price = dto.Price,
                OriginalPrice = dto.OriginalPrice,
                ListingType = dto.ListingType,
                Description = dto.Description,
                SellerId = sellerId,
                Status = "Pending",

                Specification = new Specification
                {
                    Storage = dto.Storage,
                    Ram = dto.Ram,
                    Display = dto.Display,
                    Processor = dto.Processor,
                    Camera = dto.Camera,
                    Battery = dto.Battery,
                    OS = dto.OS
                }
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // ================= IMAGE UPLOAD =================

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var images = new List<ProductImage>();

            if (dto.Images != null && dto.Images.Any())
            {
                foreach (var file in dto.Images)
                {
                    if (file == null || file.Length == 0)
                        continue;

                    var ext = Path.GetExtension(file.FileName);
                    var fileName = Guid.NewGuid() + ext;
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    images.Add(new ProductImage
                    {
                        Url = "/uploads/" + fileName,
                        ProductId = product.Id
                    });
                }
            }

            if (images.Any())
            {
                _context.ProductImages.AddRange(images);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Product submitted for approval",
                productId = product.Id
            });
        }

        // ================= GET ALL PRODUCTS =================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var baseUrl = "https://xowner-backend-1.onrender.com";

            var products = await _context.Products
                .Where(p => p.Status == "Approved")
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Select(p => new
                {
                    p.Id,
                    p.Title,
                    p.Category,
                    p.Brand,
                    p.Model,
                    p.Condition,
                    p.Price,
                    p.OriginalPrice,
                    p.ListingType,
                    p.Description,

                    Seller = new
                    {
                        p.Seller.Id,
                        p.Seller.Name,
                        p.Seller.ShopName,
                        p.Seller.Phone
                    },

                    Specification = new
                    {
                        p.Specification.Storage,
                        p.Specification.Ram,
                        p.Specification.Display,
                        p.Specification.Processor,
                        p.Specification.Camera,
                        p.Specification.Battery,
                        p.Specification.OS
                    },

                    Images = p.Images.Select(i =>
                        $"{baseUrl}{i.Url}"
                    ).ToList()
                })
                .ToListAsync();

            return Ok(products);
        }
    }
}