using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class MetricPart
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("MetricId")]
        public Metric Metric { get; set; }
        public int MetricId { get; set; }
        [Required]
        public string EventName { get; set; }
        public string EventProperty { get; set; }
    }
}
