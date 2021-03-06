﻿using Analytics.Entities;
using Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Analytics.Services
{
    public class EventRepository : BaseRepository, IEventRepository
    {
        private IProjectRepository projectRepository;
        private IProjectUserRepository projectUserRepository;
        private IPropertyRepository propertyRepository;

        public EventRepository(AnalyticsContext context, IProjectRepository projectRepository, IProjectUserRepository projectUserRepository, IPropertyRepository propertyRepository)
        {
            this.context = context;
            this.projectRepository = projectRepository;
            this.projectUserRepository = projectUserRepository;
            this.propertyRepository = propertyRepository;
        }

        public Event AddEvent(EventForCreationDto e, int projectUserId, int sessionId)
        {
            var finalEvent = new Event
            {
                SessionId = sessionId,
                Name = e.Name,
                Date = DateTime.Now
            };
            context.Events.Add(finalEvent);
            if(!Save())
            {
                return null;
            }

            // Also store event properties
            List<Property> finalProperties = new List<Property>();
            foreach (var p in e.Properties)
            {
                finalProperties.Add(new Property
                {
                    EventId = finalEvent.Id,
                    Name = p.Name,
                    Value = p.Value,
                    DataType = p.DataType
                });
            }                   
            context.Properties.AddRange(finalProperties);
            return finalEvent;
        }

        public List<EventDto> GetEventsFor(List<Session> sessions, int limit, bool withProperties = false)
        {
            var finalEvents = new List<EventDto>();
            // Get sessions
            foreach (var session in sessions)
            {
                // Get events from sessions and add to finalEvents
                var events = context.Events.Where(e => e.SessionId == session.Id).ToList();
                foreach (var e in events)
                {
                    var properties = propertyRepository.GetPropertiesForEvent(e.Id);
                    var propertiesDto = new List<PropertyDto>();
                    // Add properties if applicable
                    if (withProperties)
                    {
                        foreach (var p in properties)
                        {
                            propertiesDto.Add(new PropertyDto
                            {
                                Id = p.Id,
                                Name = p.Name,
                                DataType = p.DataType,
                                Value = p.Value
                            });
                        }
                    }

                    finalEvents.Add(new EventDto()
                    {
                        Id = e.Id,
                        Name = e.Name,
                        CreatedBy = projectUserRepository.GetProjectUser(session.ProjectUserId).Username,
                        Date = e.Date,
                        Properties = propertiesDto
                    });
                    
                    if (finalEvents.Count >= limit)
                    {
                        return finalEvents;
                    }
                }
            }
            return finalEvents;                       
        }

        public Event GetEvent(int id)
        {
            return context.Events.Where(e => e.Id == id).SingleOrDefault();
        }

        public List<Event> GetEventsFor(Session session, bool withProperties = false)
        {
            return context.Events.Where(e => e.SessionId == session.Id).ToList();
        }
    }
}
