//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.EntityFrameworkCore;
//using System.Security.Claims;
//using XownerWebOne.Data;
//using XownerWebOne.DTOs;
//using XownerWebOne.Models;

//namespace XownerWebOne.Controllers
//{
//    [Authorize]
//    [ApiController]
//    [Route("api/[controller]")]
//    public class ProductsController : ControllerBase
//    {
//        private readonly AppDbContext _context;

//        public ProductsController(AppDbContext context)
//        {
//            _context = context;
//        }

//        // ================= CREATE PRODUCT =================
//        [HttpPost]
//        public async Task<IActionResult> Create([FromForm] ProductCreateDto dto)
//        {
//            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

//            if (userId == null)
//                return Unauthorized("Login required");

//            var product = new Product
//            {
//                Title = dto.Title,
//                Category = dto.Category,
//                Brand = dto.Brand,
//                Model = dto.Model,
//                Condition = dto.Condition,
//                Price = dto.Price,
//                OriginalPrice = dto.OriginalPrice,
//                ListingType = dto.ListingType,
//                Description = dto.Description,
//                SellerId = int.Parse(userId),
//                Status = "Pending",

//                Specification = new Specification
//                {
//                    Storage = dto.Storage,
//                    Ram = dto.Ram,
//                    Display = dto.Display,
//                    Processor = dto.Processor,
//                    Camera = dto.Camera,
//                    Battery = dto.Battery,
//                    OS = dto.OS
//                }
//            };

//            _context.Products.Add(product);
//            await _context.SaveChangesAsync();

//            // ================= IMAGE UPLOAD =================
//            if (dto.Images == null)
//                dto.Images = new List<IFormFile>();

//            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

//            if (!Directory.Exists(uploadFolder))
//            {
//                Directory.CreateDirectory(uploadFolder);
//            }

//            var images = new List<ProductImage>();

//            foreach (var file in dto.Images)
//            {
//                if (file == null || file.Length == 0)
//                    continue;

//                var ext = Path.GetExtension(file.FileName);
//                var fileName = Guid.NewGuid() + ext;
//                var filePath = Path.Combine(uploadFolder, fileName);

//                try
//                {
//                    using (var stream = new FileStream(filePath, FileMode.Create))
//                    {
//                        await file.CopyToAsync(stream);
//                    }

//                    Console.WriteLine("Saved Image: " + fileName);

//                    images.Add(new ProductImage
//                    {
//                        Url = "/uploads/" + fileName,
//                        ProductId = product.Id
//                    });
//                }
//                catch (Exception ex)
//                {
//                    Console.WriteLine("Image upload error: " + ex.Message);
//                }
//            }

//            if (images.Any())
//            {
//                _context.ProductImages.AddRange(images);
//                await _context.SaveChangesAsync();
//            }

//            return Ok(new
//            {
//                message = "Product submitted for approval",
//                productId = product.Id
//            });
//        }

//        // ================= GET PENDING =================
//        [HttpGet("pending")]
//        public async Task<IActionResult> GetPendingProducts()
//        {
//            var products = await _context.Products
//                .Where(p => p.Status == "Pending")
//                .Include(p => p.Images)
//                .Include(p => p.Seller)
//                .ToListAsync();

//            return Ok(products);
//        }

//        // ================= APPROVE =================
//        [HttpPut("approve/{id}")]
//        public async Task<IActionResult> Approve(int id)
//        {
//            var product = await _context.Products.FindAsync(id);

//            if (product == null)
//                return NotFound();

//            product.Status = "Approved";
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Product approved" });
//        }

//        // ================= REJECT =================
//        [HttpPut("reject/{id}")]
//        public async Task<IActionResult> Reject(int id)
//        {
//            var product = await _context.Products.FindAsync(id);

//            if (product == null)
//                return NotFound();

//            product.Status = "Rejected";
//            await _context.SaveChangesAsync();

//            return Ok(new { message = "Product rejected" });
//        }

//        // ================= GET ALL PRODUCTS =================
//        [AllowAnonymous]
//        [HttpGet]
//        public async Task<IActionResult> GetAllProducts()
//        {
//            var baseUrl = "https://xowner-backend-1.onrender.com";

//            var products = await _context.Products
//                .Where(p => p.Status == "Approved")
//                .Include(p => p.Images)
//                .Include(p => p.Seller)
//                .Select(p => new
//                {
//                    p.Id,
//                    p.Title,
//                    p.Category,
//                    p.Brand,
//                    p.Model,
//                    p.Condition,
//                    p.Price,
//                    p.OriginalPrice,
//                    p.ListingType,
//                    p.Description,

//                    Seller = new
//                    {
//                        p.Seller.Id,
//                        p.Seller.Name,
//                        p.Seller.ShopName,
//                        p.Seller.Phone
//                    },

//                    Specification = new
//                    {
//                        p.Specification.Storage,
//                        p.Specification.Ram,
//                        p.Specification.Display,
//                        p.Specification.Processor,
//                        p.Specification.Camera,
//                        p.Specification.Battery,
//                        p.Specification.OS
//                    },

//                    Images = p.Images.Select(i =>
//                        $"{baseUrl}{i.Url}"
//                    ).ToList()
//                })
//                .ToListAsync();

//            return Ok(products);
//        }

//        // ================= DELETE =================
//        [HttpDelete("{id}")]
//        public async Task<IActionResult> Delete(int id)
//        {
//            var userId = int.Parse(
//                User.FindFirstValue(ClaimTypes.NameIdentifier)!
//            );

//            var product = await _context.Products
//                .Include(p => p.Images)
//                .FirstOrDefaultAsync(p => p.Id == id);

//            if (product == null)
//                return NotFound("Product not found");

//            if (product.SellerId != userId)
//                return Forbid("You are not the owner");

//            if (product.Images != null)
//            {
//                foreach (var img in product.Images)
//                {
//                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.Url.TrimStart('/'));
//                    if (System.IO.File.Exists(path))
//                        System.IO.File.Delete(path);
//                }
//            }

//            _context.Products.Remove(product);
//            await _context.SaveChangesAsync();

//            return Ok("Product deleted successfully");
//        }
//    }
//}
//
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

                // ✅ FIX 1: SellerId token se lo, frontend se nahi
                SellerId = int.Parse(userId),

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

            if (dto.Images == null)
                dto.Images = new List<IFormFile>();

            var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

            if (!Directory.Exists(uploadFolder))
                Directory.CreateDirectory(uploadFolder);

            var images = new List<ProductImage>();

            foreach (var file in dto.Images)
            {
                if (file == null || file.Length == 0)
                    continue;

                var ext = Path.GetExtension(file.FileName);
                var fileName = Guid.NewGuid() + ext;
                var filePath = Path.Combine(uploadFolder, fileName);

                try
                {
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
                catch (Exception ex)
                {
                    return StatusCode(500, ex.Message);
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

        // ================= GET PENDING =================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPendingProducts()
        {
            var products = await _context.Products
                .Where(p => p.Status == "Pending")
                .Include(p => p.Images)
                .ToListAsync();

            return Ok(products);
        }

        // ================= APPROVE =================
        [HttpPut("approve/{id}")]
        public async Task<IActionResult> Approve(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.Status = "Approved";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Product approved" });
        }

        // ================= REJECT =================
        [HttpPut("reject/{id}")]
        public async Task<IActionResult> Reject(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();
            product.Status = "Rejected";
            await _context.SaveChangesAsync();
            return Ok(new { message = "Product rejected" });
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
                .Include(p => p.Specification)
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

                    // ✅ FIX 2: SellerId return karo chat ke liye
                    p.SellerId,

                    // ✅ FIX 3: User table se seller info lo
                    Seller = new
                    {
                        Id = p.SellerId,
                        Name = _context.Users
                            .Where(u => u.Id == p.SellerId)
                            .Select(u => u.FullName)
                            .FirstOrDefault() ?? "Unknown",
                        Phone = _context.Users
                            .Where(u => u.Id == p.SellerId)
                            .Select(u => u.Phone)
                            .FirstOrDefault() ?? ""
                    },

                    Specification = p.Specification == null ? null : new
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

        // ================= DELETE =================
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
                return Forbid("You are not the owner");

            if (product.Images != null)
            {
                foreach (var img in product.Images)
                {
                    var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", img.Url.TrimStart('/'));
                    if (System.IO.File.Exists(path))
                        System.IO.File.Delete(path);
                }
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return Ok("Product deleted successfully");
        }

        // ================= GET PRODUCT BY ID =================
        [AllowAnonymous]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var baseUrl = "https://xowner-backend-1.onrender.com";

            var product = await _context.Products
                .Include(p => p.Images)
                .Include(p => p.Specification)
                .Where(p => p.Id == id)
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

                    // ✅ FIX 4: SellerId return karo chat ke liye
                    p.SellerId,

                    // ✅ FIX 5: User table se seller info lo
                    Seller = new
                    {
                        Id = p.SellerId,
                        Name = _context.Users
                            .Where(u => u.Id == p.SellerId)
                            .Select(u => u.FullName)
                            .FirstOrDefault() ?? "Unknown",
                        Phone = _context.Users
                            .Where(u => u.Id == p.SellerId)
                            .Select(u => u.Phone)
                            .FirstOrDefault() ?? ""
                    },

                    Specification = p.Specification == null ? null : new
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
                .FirstOrDefaultAsync();

            if (product == null)
                return NotFound();

            return Ok(product);
        }
    }
}