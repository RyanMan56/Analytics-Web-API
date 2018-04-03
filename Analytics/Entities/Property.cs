using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Property
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [ForeignKey("EventId")]
        public Event Event { get; set; }
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Value { get; set; }
        public string DataType { get; set; }
    }
}
