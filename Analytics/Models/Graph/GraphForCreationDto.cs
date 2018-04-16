using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models.Graph
{
    public class GraphForCreationDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string MetricName { get; set; }
    }
}
