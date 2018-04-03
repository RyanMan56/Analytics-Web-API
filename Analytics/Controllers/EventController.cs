using Analytics.Models;
using Analytics.Services;
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

        public EventController(IEventRepository eventRepository, IProjectRepository projectRepository) {
            this.eventRepository = eventRepository;
            this.projectRepository = projectRepository;
        }

        [Authorize, HttpPost("{id}/event")]
        public IActionResult AddEvent(int id, [FromBody] EventForCreationDto e) // e since event is a keyword in c#
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var userId = int.Parse(User.GetClaim(OpenIdConnectConstants.Claims.Subject));

            var project = projectRepository.GetProject(id, true);
            if (project == null)
            {
                return StatusCode(500, Messages.ErrorMessages.projectNotFound);
            }
            if (!projectRepository.IsUserAnalyserOfProject(userId, project))
            {
                return Unauthorized();
            }

            var finalEvent = eventRepository.AddEvent(e, userId, id);
            if (finalEvent == null)
            {
                return StatusCode(500, Messages.ErrorMessages.generic);
            }
            if (!eventRepository.Save())
            {
                return StatusCode(500, Messages.ErrorMessages.save);
            }
            return Ok(finalEvent);
        }
    }
}
