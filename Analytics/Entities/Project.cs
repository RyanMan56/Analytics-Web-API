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
        public List<ProjectAnalyser> ProjectAnalysers { get; set; }
        [Required]
        public string Url { get; set; }
        [Required]
        public string ApiKey { get; set; }
        public List<Metric> Metrics { get; set; }
        public List<ProjectUser> ProjectUsers { get; set; }
        public List<Session> Sessions { get; set; }
        public List<Graph> Graphs { get; set; }
    }
}
