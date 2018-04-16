using Analytics.Entities;
using Analytics.Models;
using Analytics.Services;
using Analytics.Utils;
using AspNet.Security.OpenIdConnect.Extensions;
using AspNet.Security.OpenIdConnect.Primitives;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ProjectUserController : Controller
    {
        private IProjectRepository projectRepository;
        private IProjectUserRepository projectUserRepository;
        private ISessionRepository sessionRepository;
        private IEventRepository eventRepository;

        public ProjectUserController(IProjectRepository projectRepository, IProjectUserRepository projectUserRepository, ISessionRepository sessionRepository, IEventRepository eventRepository)
        {
            this.projectRepository = projectRepository;
            this.projectUserRepository = projectUserRepository;
            this.sessionRepository = sessionRepository;
            this.eventRepository = eventRepository;
        }

        [Authorize, HttpGet("{projectId}/user/{projectUserId}/usage")]
        public IActionResult GetTotalUsage(int projectId, int projectUserId, DateTime? fromDate = null)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var finalFromDate = fromDate;
            if (fromDate == null)
            {
                // Subtract a month from now, since we want to get usage over the past month by default
                finalFromDate = DateTime.Now.AddMonths(-1);
            }

            var project = projectRepository.GetProject(projectId, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            if (userRole != Roles.Analyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var totalUsage = projectUserRepository.GetUsage(projectUserId, finalFromDate.Value, sessionRepository, eventRepository);

            return Ok(totalUsage);
        }

        [Authorize, HttpGet("{projectId}/user")]
        public IActionResult GetProjectUsers(int projectId, DateTime? fromDate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var finalFromDate = fromDate;
            if (fromDate == null)
            {
                // Subtract a month from now, since we want to get usage over the past month by default
                finalFromDate = DateTime.Now.AddMonths(-1);
            }

            var project = projectRepository.GetProject(projectId, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            if (userRole != Roles.Analyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }
            
            var projectUsers = projectRepository.GetProjectUsersOfProject(project);
            var projectUsersDto = new List<ProjectUserDto>();
            foreach (var projectUser in projectUsers)
            {
                projectUsersDto.Add(new ProjectUserDto
                {
                    Username = projectUser.Username,
                    LastActive = projectUser.LastActive,
                    Usage = projectUserRepository.GetUsage(projectUser.Id, finalFromDate.Value, sessionRepository, eventRepository)
                });
            }
            return Ok(projectUsersDto);
        }
    }
}
