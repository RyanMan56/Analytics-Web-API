using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Graph
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }

        [ForeignKey("ProjectId")]
        [InverseProperty("Graphs")]
        public Project Project { get; set; }
        public int? ProjectId { get; set; }

        [ForeignKey("MetricId")]
        public Metric Metric { get; set; }
        public int? MetricId { get; set; }
    }
}
