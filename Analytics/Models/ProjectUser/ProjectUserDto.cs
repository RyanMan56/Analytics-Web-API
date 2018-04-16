using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class ProjectUserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public DateTime LastActive { get; set; }
        public double Usage { get; set; }
    }
}
