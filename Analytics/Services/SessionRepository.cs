using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;

namespace Analytics.Services
{
    public class SessionRepository : BaseRepository, ISessionRepository
    {
        private IEventRepository eventRepository;
        
        public SessionRepository(AnalyticsContext context, IEventRepository eventRepository)
        {
            this.context = context;
            this.eventRepository = eventRepository;
        }

        public List<Session> GetSessionsForProject(int projectId)
        {
            return context.Sessions.Where(s => s.ProjectId == projectId).ToList();
        }

        public Session GetSession(int id)
        {
            return context.Sessions.Where(s => s.Id == id).SingleOrDefault();
        }

        public DateTime GetTotalUsage(int projectId, DateTime fromDate)
        {
            var sessions = GetSessionsForProject(projectId);
            DateTime totalUsage = new DateTime(0);
            foreach (var session in sessions)
            {
                List<Event> events = eventRepository.GetEventsFor(session).Where(e => e.Date > fromDate).OrderBy(e => e.Date).ToList();
                totalUsage += events[events.Count - 1].Date - events[0].Date;
            }

            return totalUsage;
        }
    }
}
