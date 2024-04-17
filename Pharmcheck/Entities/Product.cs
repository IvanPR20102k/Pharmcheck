using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmcheck.Entities
{
    public class Product
    {
        public int ID { get; set; }
        public string ShopID { get; set; } = null!;
        public string Name { get; set; } = null!;
        public float PriceMin { get; set; }
        public float PriceMax { get; set; }
        public byte Status { get; set; }

        public int ImportID { get; set; }
        public Import Import { get; set; } = null!;

        public List<Comparison>? Comparisons { get; } = [];
    }
}
