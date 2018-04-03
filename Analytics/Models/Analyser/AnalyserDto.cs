using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class AnalyserDto
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public int UserId { get; set; }
        public int ProjectId { get; set; }
    }
}
