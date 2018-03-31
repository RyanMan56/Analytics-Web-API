using Analytics.Models.User;
using Analytics.Services;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Controllers
{
    [Route("api/users")]
    public class UserController : Controller
    {
        private IUserRepository userRepository;

        public UserController(IUserRepository userRepository)
        {
            this.userRepository = userRepository;
        }
        
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserForCreationDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (!userRepository.Create(user.Email, user.Name, user.Password))
            {
                return StatusCode(500, "Your account was not created.");
            }

            if (!userRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
                        
            return Ok(ModelState);
        }

        [HttpGet()]
        public IActionResult Login([FromQuery] string email, [FromQuery] string password)
        {
            var user = userRepository.ValidatePassword(email, password);
            if (user == null)
            {
                return StatusCode(500, "Login failed.");           
            }
            return Ok("Logged in! Welcome, " + user.Name);
        }
    }
}
