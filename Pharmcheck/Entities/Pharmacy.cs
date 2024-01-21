using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pharmcheck.Entities
{
    public class Pharmacy
    {
        public int ID { get; set; }
        public string Name { get; set; } = null!;

        public List<Import>? Imports { get; } = [];
    }
}
