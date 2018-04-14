using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Analytics.Models;
using Analytics.Services;
using Microsoft.AspNetCore.Authorization;
using AspNet.Security.OpenIdConnect.Primitives;
using AspNet.Security.OpenIdConnect.Extensions;

namespace Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ProjectController : Controller
    {
        private IProjectRepository projectRepository;
        private IProjectUserRepository projectUserRepository;
        private ISessionRepository sessionRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectUserRepository projectUserRepository, ISessionRepository sessionRepository)
        {
            this.projectRepository = projectRepository;
            this.projectUserRepository = projectUserRepository;
            this.sessionRepository = sessionRepository;
        }

        [Authorize, HttpPost("create")]
        public IActionResult CreateProject([FromBody] ProjectForCreationDto project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var url = $"{Request.Scheme}://{Request.Host }{Request.Path}";
            url = url.Remove(url.Length - 6);
            var finalProject = projectRepository.Create(project.Name, project.Password, project.Analysers, url);
            if (finalProject == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!projectRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            //return Ok("Project created.");
            return Ok("Project with ID of " + finalProject.Id + " created.");
        }

        [Authorize, HttpGet("{id}")]
        public IActionResult GetDetails(int id)
        {            
            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }            
            return Ok(AutoMapper.Mapper.Map<ProjectDetailsDto>(project));
        }

        [Authorize, HttpGet("{id}/users")]
        public IActionResult GetProjectUsers(int id)
        {
            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            return Ok(projectUserRepository.GetProjectUsers(project.Id));
        }

        [Authorize, HttpGet("{id}/usage")]
        public IActionResult GetProjectUsage(int id, DateTime? fromDate = null)
        {
            var finalFromDate = fromDate;
            if (fromDate == null)
            {
                // Subtract a month from now, since we want to get usage over the past month by default
                finalFromDate = DateTime.Now.AddMonths(-1);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }
            
            return Ok(sessionRepository.GetTotalUsage(id, finalFromDate.Value));
        }
    }
}