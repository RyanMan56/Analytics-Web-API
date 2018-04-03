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

        [ForeignKey("ProjectId")]
        [InverseProperty("Events")]
        public Project Project { get; set; }
        public int ProjectId { get; set; }

        [ForeignKey("ProjectUserId")]
        public ProjectUser ProjectUser { get; set; }
        public int ProjectUserId { get; set; }

        [Required]
        public string Name { get; set; }
        public List<Property> Properties { get; set; }
    }
}
