using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Models
{
    public class UserResetPasswordDto
    {
        [Required(ErrorMessage = "Please enter a valid email.")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Please enter a valid security answer.")]
        public string SecurityAnswer { get; set; }
        [Required(ErrorMessage = "Please enter a valid new password.")]
        public string NewPassword { get; set; }
    }
}
