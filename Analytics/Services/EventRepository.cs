using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;
using Analytics.Models;
using System.Diagnostics;

namespace Analytics.Services
{
    public class EventRepository : BaseRepository, IEventRepository
    {
        private IProjectRepository projectRepository;

        public EventRepository(AnalyticsContext context, IProjectRepository projectRepository)
        {
            this.context = context;
            this.projectRepository = projectRepository;
        }

        public Event AddEvent(EventForCreationDto e, int puid, int pid)
        {
            var finalEvent = new Event
            {
                ProjectId = pid,
                ProjectUserId = puid,
                Name = e.Name
            };
            context.Events.Add(finalEvent);
            if(!Save())
            {
                return null;
            }

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
            //AutoMapper.Mapper.Map<List<Property>>(e.Properties);
            
            //var finalEvent = AutoMapper.Mapper.Map<Event>(e);                        
            context.Properties.AddRange(finalProperties);
            return finalEvent;
        }
    }
}
