using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Session
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [ForeignKey("ProjectUserId")]
        public ProjectUser ProjectUser;
        public int ProjectUserId { get; set; }

        [ForeignKey("ProjectId")]
        [InverseProperty("Sessions")]
        public Project Project { get; set; }
        public int ProjectId { get; set; }

        public List<Event> Events { get; set; }
    }
}
