﻿using Analytics.Entities;
using Analytics.Models;
using Analytics.Models.Property;
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
            // Get user's role from token
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);

            if (userRole != Roles.ProjectUser)
            {
                return StatusCode(403, "User does not belong to a project.");
            }

            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            var sessionId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Issuer));            

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsProjectUserOfProject(userId, project))
            {
                return Unauthorized();
            }

            var finalEvent = eventRepository.AddEvent(e, userId, sessionId);
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

        [Authorize, HttpPost("event")]
        public IActionResult CreateEvent([FromBody] EventForCreationDto e)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userRole = User.GetClaim(OpenIdConnectConstants.Claims.Role);
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));
            var sessionId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Issuer));

            if (userRole != Roles.ProjectUser)
            {
                return StatusCode(403, "User does not belong to a project.");
            }

            var apiKey = e.ApiKey;

            var project = projectRepository.GetProjectByApiKey(apiKey);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsProjectUserOfProject(userId, project))
            {
                return Unauthorized();
            }

            var finalEvent = eventRepository.AddEvent(e, userId, sessionId);
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

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsAnalyserOfProject(userId, project))
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

            if (userRole != Roles.Analyser)
            {
                return StatusCode(403, Messages.ErrorMessages.userNotAnalyser);
            }

            var e = eventRepository.GetEvent(id);
            if (e == null)
            {
                return StatusCode(500, Messages.ErrorMessages.eventNotFound);
            }
            var session = sessionRepository.GetSession(e.SessionId);
            if (session.ProjectUserId != userId)
            {
                return Unauthorized();
            }

            var properties = propertyRepository.GetPropertiesForEvent(eventId);
            return Ok(AutoMapper.Mapper.Map<List<PropertyForDisplayDto>>(properties));
        }
    }    
}
