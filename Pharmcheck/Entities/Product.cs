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
        public required string Name { get; set; }
        public int PriceMin { get; set; }
        public int PriceMax { get; set; }

        public required int ImportID { get; set; }
        public required Import Import { get; set; }

        public List<Comparison>? Comparisons { get; } = [];
    }
}
