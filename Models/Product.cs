namespace XownerWebOne.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string Title { get; set; }
        public string Category { get; set; }
        public string Brand { get; set; }
        public string Model { get; set; }
        public string Condition { get; set; }

        public decimal Price { get; set; }

        public decimal OriginalPrice { get; set; }
        public string ListingType { get; set; }
        public string Description { get; set; }

        public int SellerId { get; set; }
        public User Seller { get; set; } // ✅ Seller ki jagah User use karo
        public string Status { get; set; } = "Pending";   // NEW


        // ✅ Embedded Specification (NO SEPARATE TABLE)
        public Specification Specification { get; set; }

        public List<ProductImage> Images { get; set; }
    }
}
