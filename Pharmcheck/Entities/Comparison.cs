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
        public DateTime DateTime { get; set; }
        public decimal Price { get; set; }
        public bool IsOutOfBounds { get; set; }
        public int ShopsAmount { get; set; }

        public required int ProductID { get; set; }
        public required Product Product { get; set; }
    }
}
