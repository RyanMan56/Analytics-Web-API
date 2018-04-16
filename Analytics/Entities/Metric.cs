using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Metric
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public List<MetricPart> MetricsParts { get; set; }
        [Required]
        public string MetricType { get; set; }
        [Required]
        [ForeignKey("ProjectId")]
        public Project Project { get; set; }
        public int ProjectId { get; set; }
    }
}
