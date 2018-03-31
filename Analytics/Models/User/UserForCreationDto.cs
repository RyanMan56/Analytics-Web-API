using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models.User
{
    public class UserForCreationDto
    {
        [Required(ErrorMessage = "Please provide a valid email")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please provide a valid name")]
        [MaxLength(200)]
        public string Name { get; set; }
        [Required]
        public string Password { get; set; }
        public string PasswordSalt { get; set; }
    }
}
