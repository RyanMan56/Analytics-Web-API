using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Event
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("SessionId")]
        [InverseProperty("Events")]
        public Session Session { get; set; }
        public int SessionId { get; set; }

        [Required]
        public string Name { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public List<Property> Properties { get; set; }
    }
}
