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
    [Route("api")]
    public class EventController : Controller
    {
        private IEventRepository eventRepository;
        private IProjectRepository projectRepository;
        private ISessionRepository sessionRepository;
        private IPropertyRepository propertyRepository;
        private IProjectUserRepository projectUserRepository;

        public EventController(IEventRepository eventRepository, IProjectRepository projectRepository, ISessionRepository sessionRepository, IPropertyRepository propertyRepository, IProjectUserRepository projectUserRepository) {
            this.eventRepository = eventRepository;
            this.projectRepository = projectRepository;
            this.sessionRepository = sessionRepository;
            this.propertyRepository = propertyRepository;
            this.projectUserRepository = projectUserRepository;
        }

        [Authorize, HttpPost("{id}/event")]
        public IActionResult AddEvent(int id, [FromBody] EventForCreationDto e) // e since event is a keyword in c#
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.ProjectUser)
            {
                return StatusCode(403, "User does not belong to a project.");
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsProjectUserOfProject(userId, project))
            {
                return Unauthorized();
            }

            var finalEvent = eventRepository.AddEvent(e, userId, id);
            if (finalEvent == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            projectUserRepository.UpdateLastActive(userId);            
            if (!eventRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(finalEvent);
        }

        [Authorize, HttpGet("{id}/event")]
        public IActionResult GetEvents(int id, int limit = 100)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.ProjectUser)
            {
                return StatusCode(403, "User does not belong to a project.");
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsProjectUserOfProject(userId, project))
            {
                return Unauthorized();
            }
            project.Sessions = sessionRepository.GetSessionsForProject(project.Id);

            var events = eventRepository.GetEventsFor(project.Sessions, limit);
            if (events == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            return Ok(events);
        }

        [Authorize, HttpGet("{id}/event/{eventId}/properties")]
        public IActionResult GetPropertiesForEvent(int id, int eventId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            if (userRole != Roles.ProjectUser)
            {
                return StatusCode(403, "User does not belong to a project.");
            }

            var e = eventRepository.GetEvent(id);
            var session = sessionRepository.GetSession(e.SessionId);
            if (session.ProjectUserId != userId)
            {
                return Unauthorized();
            }

            var properties = propertyRepository.GetPropertiesForEvent(eventId);
            return Ok(AutoMapper.Mapper.Map<List<PropertyDto>>(properties));
        }
    }    
}
