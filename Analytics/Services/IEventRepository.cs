using Analytics.Entities;
using Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IEventRepository
    {
        Event AddEvent(EventForCreationDto e, int projectUserId, int sessionId);
        List<EventDto> GetEventsFor(List<Session> sessions, int limit, bool withProperties = false);
        List<Event> GetEventsFor(Session session, bool withProperties = false);
        Event GetEvent(int id);
        bool Save();
    }
}
