using Analytics.Models;
using Analytics.Services;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Server;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api/users")]
    public class UserController : Controller
    {
        private IUserRepository userRepository;
        private IProjectUserRepository projectUserRepository;
        private IProjectRepository projectRepository;

        public UserController(IUserRepository userRepository, IProjectUserRepository projectUserRepository, IProjectRepository projectRepository)
        {
            this.userRepository = userRepository;
            this.projectUserRepository = projectUserRepository;
            this.projectRepository = projectRepository;
        }
        
        [HttpPost("register")]
        public IActionResult Register([FromBody] UserForCreationDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (!userRepository.Create(user.Username, user.Name, user.Password, user.SecurityQuestion, user.SecurityAnswer))
            {
                return StatusCode(500, "Your account was not created.");
            }

            if (!userRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
                        
            return Ok(ModelState);
        }       

        [HttpPost("login")]
        public IActionResult Login(OpenIdConnectRequest request, bool analyser)
        {
            if (analyser)
            {
                return AnalyserLogin(request);
            }
            return StartSession(request);
        }

        public IActionResult AnalyserLogin(OpenIdConnectRequest request)
        {
            if (!request.IsPasswordGrantType())
            {
                return StatusCode(500, "Incorrect grant type");
            }

            var user = userRepository.ValidatePassword(request.Username, request.Password);
            if (user == null)
            {
                // Return a 401 unauthorized response if user was not validated
                return StatusCode(401, "Email or password is invalid.");
            }

            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);

            identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                "" + user.Id,
                OpenIdConnectConstants.Destinations.AccessToken);

            identity.AddClaim(OpenIdConnectConstants.Claims.Name,
                user.Name,
                OpenIdConnectConstants.Destinations.AccessToken);

            var principal = new ClaimsPrincipal(identity);

            // New token generated and OAuth2 token response returned
            return SignIn(principal, OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        public IActionResult StartSession(OpenIdConnectRequest request)
        {
            var apiKey = request.Username;
            var password = request.Password;
            var username = request.Display;
            // decode api key and get 
            var project = projectRepository.GetProjectByApiKey(apiKey);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.ValidateProjectPassword(password, project))
            {
                return Unauthorized();
            }
            var projectUser = projectUserRepository.GetProjectUser(username, project);            
            if (projectUser == null)
            {                
                projectUser = projectUserRepository.CreateProjectUser(username, project);
            }
            if (projectUser == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!projectUserRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok("Session started.");
            // Create new session
        }

        [Authorize, HttpDelete]
        public IActionResult Delete()
        {
            if (!userRepository.Delete(int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject))))
            {
                return StatusCode(500, "A problem happened while deleting user.");
            }
            if (!userRepository.Save())
            {
                return StatusCode(500, "A problem happened while saving your request.");
            }
            return Ok(User.GetClaim(OpenIdConnectConstants.Claims.Name) + "'s account has been deleted.");
        }

        [HttpPost("reset-password")]
        public IActionResult ResetPassword([FromBody] UserResetPasswordDto user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!userRepository.ResetPassword(user.Email, user.SecurityAnswer, user.NewPassword))
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
            if (!userRepository.Save())
            {
                return StatusCode(500, "A problem happened while saving your request.");
            }
            return Ok("Password reset.");
        }
    }
}
