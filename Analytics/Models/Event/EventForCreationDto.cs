using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class EventForCreationDto
    {
        [Required]
        public string Name { get; set; }
        public List<PropertyDto> Properties { get; set; }
    }
}
