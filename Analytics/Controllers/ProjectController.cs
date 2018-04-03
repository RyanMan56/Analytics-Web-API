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

        public ProjectController(IProjectRepository projectRepository)
        {
            this.projectRepository = projectRepository;
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
            if (!projectRepository.Create(project.Name, project.Password, project.Analysers, url))
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!projectRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            //return Ok("Project created.");
            return Ok(this.Request.Scheme);
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
            bool userIsAnalyser = projectRepository.IsUserAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }            
            return Ok(AutoMapper.Mapper.Map<ProjectDetailsDto>(project));
        }
    }
}