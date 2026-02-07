using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using XownerWebOne.Data;
using XownerWebOne.DTOs;
using XownerWebOne.Models;

namespace XownerWebOne.Controllers
{
    [Authorize] // 🔐 Login required by default
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ProductsController(AppDbContext context)
        {
            _context = context;
        }

        // ================= CREATE PRODUCT (SELLER) =================
        [HttpPost]
        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
                return Unauthorized("Login required");

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

                SellerId = int.Parse(userId),

                // 🔥 NEW — APPROVAL WORKFLOW
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

            // ================= IMAGE UPLOAD (UNCHANGED) =================
            if (dto.Images != null && dto.Images.Any())
            {
                var uploadFolder = Path.Combine("wwwroot", "uploads");

                if (!Directory.Exists(uploadFolder))
                    Directory.CreateDirectory(uploadFolder);

                var images = new List<ProductImage>();

                foreach (var file in dto.Images)
                {
                    var ext = Path.GetExtension(file.FileName);
                    var fileName = Guid.NewGuid() + ext;
                    var filePath = Path.Combine(uploadFolder, fileName);

                    using var stream = new FileStream(filePath, FileMode.Create);
                    await file.CopyToAsync(stream);

                    images.Add(new ProductImage
                    {
                        Url = "/uploads/" + fileName,
                        ProductId = product.Id
                    });
                }

                _context.ProductImages.AddRange(images);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = "Product submitted for admin approval",
                productId = product.Id
            });
        }

        // ================= ADMIN: SEE PENDING PRODUCTS =================
        [HttpGet("pending")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetPendingProducts()
        {
            var products = await _context.Products
                .Where(p => p.Status == "Pending")
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .ToListAsync();

            return Ok(products);
        }

        // ================= ADMIN: APPROVE PRODUCT =================
        [HttpPut("approve/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Status = "Approved";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product approved" });
        }

        // ================= ADMIN: REJECT PRODUCT =================
        [HttpPut("reject/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Reject(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
                return NotFound();

            product.Status = "Rejected";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product rejected" });
        }

        // ================= GET SPECIFICATION (PUBLIC) =================
        [AllowAnonymous]
        [HttpGet("{id}/specification")]
        public async Task<IActionResult> GetSpecification(int id)
        {
            var spec = await _context.Products
                .AsNoTracking()
                .Where(p => p.Id == id && p.Status == "Approved")   // 🔥 filter
                .Select(p => p.Specification)
                .FirstOrDefaultAsync();

            if (spec == null)
                return NotFound();

            return Ok(spec);
        }

        // ================= GET ALL PRODUCTS (ONLY APPROVED) =================
        [AllowAnonymous]
        [HttpGet]
        public async Task<IActionResult> GetAllProducts()
        {
            var products = await _context.Products
                .Where(p => p.Status == "Approved")   // 🔥 IMPORTANT
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

                    Images = p.Images.Select(i => i.Url).ToList()
                })
                .ToListAsync();

            return Ok(products);
        }

        // ================= GET SINGLE PRODUCT (ONLY APPROVED) =================
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Seller)
                .Where(p => p.Id == id && p.Status == "Approved")  // 🔥 filter
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

                    Images = p.Images.Select(i => i.Url).ToList()
                })
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }

        // ================= UPDATE PRODUCT (OWNER ONLY) =================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateDto dto)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var product = await _context.Products
                .Include(p => p.Specification)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Product not found");

            if (product.SellerId != userId)
                return Forbid("You are not the owner of this product");

            // ❗ If seller edits, make it pending again
            product.Status = "Pending";   // 🔥 important

            product.Title = dto.Title;
            product.Category = dto.Category;
            product.Brand = dto.Brand;
            product.Model = dto.Model;
            product.Condition = dto.Condition;
            product.Price = dto.Price;
            product.OriginalPrice = dto.OriginalPrice;
            product.ListingType = dto.ListingType;
            product.Description = dto.Description;

            product.Specification.Storage = dto.Storage;
            product.Specification.Ram = dto.Ram;
            product.Specification.Display = dto.Display;
            product.Specification.Processor = dto.Processor;
            product.Specification.Camera = dto.Camera;
            product.Specification.Battery = dto.Battery;
            product.Specification.OS = dto.OS;

            await _context.SaveChangesAsync();

            return Ok(product);
        }

        // ================= DELETE PRODUCT (OWNER ONLY) =================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!
            );

            var product = await _context.Products
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Product not found");

            if (product.SellerId != userId)
                return Forbid("You are not the owner of this product");

            if (product.Images != null)
            {
                foreach (var img in product.Images)
                {
                    var path = Path.Combine("wwwroot", img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully");
        }
    }
}
