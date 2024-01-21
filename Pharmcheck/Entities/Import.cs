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
        public string ImportDateTime { get; set; } = null!;

        public int PharmacyID { get; set; }
        public Pharmacy Pharmacy { get; set; } = null!;

        public List<Product> Products { get; } = [];
    }
}
