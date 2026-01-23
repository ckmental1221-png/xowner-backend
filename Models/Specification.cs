using Microsoft.EntityFrameworkCore;

namespace XownerWebOne.Models
{
    [Owned]
    public class Specification
    {
        public string Storage { get; set; }
        public string Ram { get; set; }
        public string Display { get; set; }
        public string Processor { get; set; }
        public string Camera { get; set; }
        public string Battery { get; set; }
        public string OS { get; set; }
    }
}
