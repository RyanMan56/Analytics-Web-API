using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface ISessionRepository
    {
        List<Session> GetSessionsForProject(int projectId);
        List<Session> GetSessionsForProject(int projectId, int projectUserId);
        double GetTotalUsage(int projectId, DateTime fromDate);
        Session GetSession(int id);
    }
}
