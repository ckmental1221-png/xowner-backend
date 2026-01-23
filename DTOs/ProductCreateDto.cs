namespace XownerWebOne.DTOs
{
    public class ProductCreateDto
    {
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

        // Specification fields
        public string Storage { get; set; }
        public string Ram { get; set; }
        public string Display { get; set; }
        public string Processor { get; set; }
        public string Camera { get; set; }
        public string Battery { get; set; }
        public string OS { get; set; }

        public List<IFormFile> Images { get; set; }
    }
}
