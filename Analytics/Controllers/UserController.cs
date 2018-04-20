using Analytics.Models;
using Analytics.Services;
using Analytics.Utils;
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

        // Repositories from dependency injection
        public UserController(IUserRepository userRepository, IProjectUserRepository projectUserRepository, IProjectRepository projectRepository)
        {
            this.userRepository = userRepository;
            this.projectUserRepository = projectUserRepository;
            this.projectRepository = projectRepository;
        }
        

        [HttpPost("register")]
        public IActionResult Register([FromBody] UserForCreationDto user)
        {
            // Check the request is valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
 
            // If response from the repository is false then return status code 500
            if (!userRepository.Create(user.Username, user.Name, user.Password, user.SecurityQuestion, user.SecurityAnswer))
            {
                return StatusCode(500, "Your account was not created.");
            }

            // If saving failed return status code 500
            if (!userRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }
                        
            // Return status code 200
            return Ok(ModelState);
        }       

        [HttpPost("login")]
        public IActionResult Login(OpenIdConnectRequest request, bool analyser)
        {
            // Is the request coming from a user with the Analyser role
            if (analyser)
            {
                return AnalyserLogin(request);
            }
            // The request is coming from a project user
            return ProjectUserLogin(request);
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

            // Set up a new claim to store user constants in the token
            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);

            // Add user id to the token
            identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                "" + user.Id,
                OpenIdConnectConstants.Destinations.AccessToken);

            // Add username to the token
            identity.AddClaim(OpenIdConnectConstants.Claims.Name,
                user.Name,
                OpenIdConnectConstants.Destinations.AccessToken);

            // Add user role to the token
            identity.AddClaim(OpenIdConnectConstants.Claims.Role,
                Roles.Analyser,
                OpenIdConnectConstants.Destinations.AccessToken);
            
            var principal = new ClaimsPrincipal(identity);

            // New token generated and OAuth2 token response returned
            return SignIn(principal, OpenIdConnectServerDefaults.AuthenticationScheme);
        }

        public IActionResult ProjectUserLogin(OpenIdConnectRequest request)
        {
            var apiKey = request.Username;
            var password = request.Password;
            var username = request.Display;
            // Get project from api key
            var project = projectRepository.GetProjectByApiKey(apiKey);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            // Check if correct password is supplied
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
            var session = projectRepository.CreateSession(project.Id, projectUser.Id);
            if (session == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!projectUserRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }

            // Create a new claim for the token
            var identity = new ClaimsIdentity(
                OpenIdConnectServerDefaults.AuthenticationScheme,
                OpenIdConnectConstants.Claims.Name,
                OpenIdConnectConstants.Claims.Role);

            // Add project user's ID as subject
            identity.AddClaim(OpenIdConnectConstants.Claims.Subject,
                "" + projectUser.Id,
                OpenIdConnectConstants.Destinations.AccessToken);

            // Project user's username as name
            identity.AddClaim(OpenIdConnectConstants.Claims.Name,
                projectUser.Username,
                OpenIdConnectConstants.Destinations.AccessToken);

            // Project user's role as role
            identity.AddClaim(OpenIdConnectConstants.Claims.Role,
                Roles.ProjectUser,
                OpenIdConnectConstants.Destinations.AccessToken);

            // Session ID as issuer
            identity.AddClaim(OpenIdConnectConstants.Claims.Issuer,
                ""+session.Id,
                OpenIdConnectConstants.Destinations.AccessToken);

            var principal = new ClaimsPrincipal(identity);

            // Create new session
            // New token generated and OAuth2 token response returned
            return SignIn(principal, OpenIdConnectServerDefaults.AuthenticationScheme);
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
