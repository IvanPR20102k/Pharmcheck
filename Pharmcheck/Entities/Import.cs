using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmcheck.Entities
{
    public class Import
    {
        public int ID { get; set; }
        public DateTime DateTime { get; set; }

        public required int PharmacyID { get; set; }
        public required Pharmacy Pharmacy { get; set; }

        public List<Product> Products { get; } = [];
    }
}
