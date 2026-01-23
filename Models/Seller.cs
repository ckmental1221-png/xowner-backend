using System.Text.Json.Serialization;

namespace XownerWebOne.Models
{
    public class Seller
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string ShopName { get; set; }
        public string Phone { get; set; }

        [JsonIgnore]
        public List<Product> Products { get; set; }
    }
}
