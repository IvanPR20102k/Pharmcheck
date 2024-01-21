using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmcheck.Entities
{
    public class Comparison
    {
        public int ID { get; set; }
        public string ComparisonDateTime { get; set; } = null!;
        public int Status { get; set; }
        public float Price { get; set; }
        public int IsOutOfBounds { get; set; }
        public int ShopsAmount { get; set; }

        public int ProductID { get; set; }
        public Product Product { get; set; } = null!;
    }
}
