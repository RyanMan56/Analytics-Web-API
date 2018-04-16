using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class PropertyDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
