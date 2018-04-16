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
using Analytics.Utils;
using Analytics.Entities;
using Analytics.Models.Graph;

namespace Analytics.Controllers
{
    [Produces("application/json")]
    [Route("api")]
    public class ProjectController : Controller
    {
        private IProjectRepository projectRepository;
        private IProjectUserRepository projectUserRepository;
        private ISessionRepository sessionRepository;
        private IMetricRepository metricRepository;
        private IUserRepository userRepository;
        private IEventRepository eventRepository;
        private IGraphRepository graphRepository;

        public ProjectController(IProjectRepository projectRepository, IProjectUserRepository projectUserRepository, ISessionRepository sessionRepository, IMetricRepository metricRepository, IUserRepository userRepository, IEventRepository eventRepository, IGraphRepository graphRepository)
        {
            this.projectRepository = projectRepository;
            this.projectUserRepository = projectUserRepository;
            this.sessionRepository = sessionRepository;
            this.metricRepository = metricRepository;
            this.userRepository = userRepository;
            this.eventRepository = eventRepository;
            this.graphRepository = graphRepository;
        }

        [Authorize, HttpPost("create")]
        public IActionResult CreateProject([FromBody] ProjectForCreationDto project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }
            var url = $"{Request.Scheme}://{Request.Host }{Request.Path}";
            url = url.Remove(url.Length - 6);
            var finalProject = projectRepository.Create(project.Name, userRepository.GetUser(userId).Name, project.Password, project.Analysers, url);
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

        [Authorize, HttpGet]
        public IActionResult GetProjects()
        {
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var projects = projectRepository.GetProjects(userId, true);
            if (projects == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            var projectDtos = new List<ProjectDetailsDto>();
            foreach (var project in projects)
            {
                var analysers = new List<Analyser>();
                foreach (var projectAnalyser in project.ProjectAnalysers)
                {
                    analysers.Add(projectAnalyser.Analyser);
                }
                projectDtos.Add(new ProjectDetailsDto
                {
                    Id = project.Id,
                    Name = project.Name,
                    Url = project.Url,
                    ApiKey = project.ApiKey,
                    Analysers = AutoMapper.Mapper.Map<List<AnalyserDto>>(analysers),
                    ProjectUsers = AutoMapper.Mapper.Map<List<ProjectUserDto>>(project.ProjectUsers)
                });
            }

            return Ok(projectDtos);
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
            var analysers = new List<Analyser>();
            foreach (var projectAnalyser in project.ProjectAnalysers)
            {
                analysers.Add(projectAnalyser.Analyser);
            }
            var projectDetailsDto = new ProjectDetailsDto
            {
                Id = project.Id,
                Name = project.Name,
                Url = project.Url,
                ApiKey = project.ApiKey,
                Analysers = AutoMapper.Mapper.Map<List<AnalyserDto>>(analysers),
                ProjectUsers = AutoMapper.Mapper.Map<List<ProjectUserDto>>(project.ProjectUsers)
            };

            return Ok(projectDetailsDto);
        }

        [Authorize, HttpPost("{id}/analyser")]
        public IActionResult AddAnalyser(string username, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
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

            var analyser = projectRepository.AddAnalyserToProject(username, project);
            if (analyser == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!projectRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(analyser);
        }

        [Authorize, HttpGet("{id}/users")]
        public IActionResult GetProjectUsers(int id, DateTime? fromDate = null)
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
            var projectUsers = projectUserRepository.GetProjectUsers(project.Id);
            var projectUserDtos = new List<ProjectUserDto>();
            foreach (var projectUser in projectUsers)
            {
                projectUserDtos.Add(new ProjectUserDto
                {
                    Id = projectUser.Id,
                    Username = projectUser.Username,
                    LastActive = projectUser.LastActive,
                    Usage = projectUserRepository.GetUsage(projectUser.Id, finalFromDate.Value, sessionRepository, eventRepository)
                });
            }

            return Ok(projectUserDtos);
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

        [Authorize, HttpPost("{id}/metric")]
        public IActionResult AddMetric([FromBody] MetricDto metricDto, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var uid = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(uid, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var metric = projectRepository.AddMetric(id, metricDto);
            if (metric == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            if (!projectRepository.Save())
            {
                return BadRequest(Messages.ErrorMessages.save);
            }

            return Ok(metric);
        }

        [Authorize, HttpGet("{id}/metric")]
        public IActionResult GetMetrics(int id, DateTime? fromDate)
        {
            var finalFromDate = DateTime.Now;
            if (fromDate != null)
            {
                finalFromDate = fromDate.Value;
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var metrics = metricRepository.GetMetrics(id, true);
            if (metrics == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            var metricDtos = new List<MetricDto>();
            foreach (var metric in metrics)
            {
                var metricPartsDto = AutoMapper.Mapper.Map<List<MetricPartDto>>(metric.MetricsParts);
                metricDtos.Add(new MetricDto
                {
                    Id = metric.Id,
                    Name = metric.Name,
                    MetricParts = metricPartsDto,
                    MetricType = metric.MetricType,
                    Value = metricRepository.CalculateMetricBeforeDate(metric, finalFromDate)
                });
            }


            return Ok(metricDtos);
        }

        [Authorize, HttpGet("{id}/metric/{metricId}")]
        public IActionResult GetMetric(int id, int metricId, DateTime? fromDate)
        {
            var finalFromDate = DateTime.Now;
            if (fromDate != null)
            {
                finalFromDate = fromDate.Value;
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var metric = metricRepository.GetMetric(metricId, id, true);
            if (metric == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            var metricPartsDto = AutoMapper.Mapper.Map<List<MetricPartDto>>(metric.MetricsParts);
            var metricDto = new MetricDto
            {
                Id = metric.Id,
                Name = metric.Name,
                MetricParts = metricPartsDto,
                MetricType = metric.MetricType,
                Value = metricRepository.CalculateMetricBeforeDate(metric, finalFromDate)
            };

            return Ok(metricDto);
        }

        [Authorize, HttpDelete("{id}/metric/{metricId}")]
        public IActionResult RemoveMetric(int id, int metricId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var metric = metricRepository.GetMetric(metricId, id, true);
            if (metric == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            metricRepository.RemoveMetric(metricId, id);

            if (!metricRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }

            return Ok("Metric " + metricId + " removed.");
        }

        [Authorize, HttpPost("{id}/metric/{metricId}")]
        public IActionResult AddMetricPart([FromBody] MetricPartDto metricPartDto, int id, int metricId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            if (!metricRepository.MetricExists(metricId, id))
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            var metric = metricRepository.AddMetricPart(metricId, id, metricPartDto);
            if (metric == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            if (!metricRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }

            return Ok(metric);
        }

        [Authorize, HttpDelete("{id}/metric/{metricId}/property/{metricPartId}")]
        public IActionResult RemoveMetricPart(int id, int metricId, int metricPartId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            if (!metricRepository.MetricExists(metricId, id))
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            metricRepository.RemoveMetricPart(metricId, id, metricPartId);

            if (!metricRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }

            return Ok("Metric part " + metricPartId + " removed.");
        }

        [Authorize, HttpPost("{id}/graph")]
        public IActionResult CreateGraph([FromBody] GraphForCreationDto graphDto, int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var graph = graphRepository.Create(graphDto, project);
            if (graph == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!graphRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(graph);
        }

        [Authorize, HttpGet("{id}/graph/{graphId}")]
        public IActionResult GetGraph(int id, int graphId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var graph = graphRepository.GetGraph(graphId);
            if (graph == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            if (!graphRepository.Save())
            {
                StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(graph);
        }

        [Authorize, HttpGet("{id}/graph")]
        public IActionResult GetGraphs(int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            var graphs = graphRepository.GetGraphsForProject(id);
            if (graphs == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }

            if (!graphRepository.Save())
            {
                StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(graphs);
        }

        [Authorize, HttpDelete("{id}/graph/{graphId}")]
        public IActionResult RemoveGraph(int id, int graphId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }

            bool userIsAnalyser = projectRepository.IsAnalyserOfProject(userId, project);
            if (!userIsAnalyser)
            {
                return StatusCode(401, Messages.ErrorMessages.userNotAnalyser);
            }

            graphRepository.DeleteGraph(graphId);

            if (!graphRepository.Save())
            {
                StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok("Graph " + graphId + " removed.");
        }
    }
}