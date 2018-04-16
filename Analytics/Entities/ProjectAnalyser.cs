using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class ProjectAnalyser
    {
        [Key]
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        [Key]
        public int AnalyserId { get; set; }
        public Analyser Analyser { get; set; }
    }
}
