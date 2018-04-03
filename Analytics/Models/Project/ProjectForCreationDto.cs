using Microsoft.Azure.KeyVault.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class ProjectForCreationDto
    {
        [Required(ErrorMessage = "Please enter a valid name.")]
        public string Name { get; set; }
        [Required(ErrorMessage = "Please enter a valid password.")]
        public string Password { get; set; }
        [Required(ErrorMessage = "Please specify at least one analyser.")]
        public List<int> Analysers { get; set; }
    }
}
