using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class Project
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string PasswordSalt { get; set; }
        [Required]
        public IEnumerable<Analyser> Analysers { get; set; } = new List<Analyser>();
        [Required]
        public string Url { get; set; }
        [Required]
        public string ApiKey { get; set; }
        public IQueryable<Metric> TrackedMetrics { get; set; }
        public List<ProjectUser> ProjectUsers { get; set; }
        public List<Event> Events { get; set; }
        //public IQueryable<Graph> Graphs { get; set; }
        //public IQueryable<DataGroup> DataGroups { get; set; }

    }
}
