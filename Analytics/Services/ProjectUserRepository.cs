using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;

namespace Analytics.Services
{
    public class ProjectUserRepository : BaseRepository, IProjectUserRepository
    {

        public ProjectUserRepository(AnalyticsContext context)
        {
            this.context = context;
        }

        public ProjectUser CreateProjectUser(string username, Project project)
        {
            ProjectUser projectUser = new ProjectUser
            {
                Username = username,
                Project = project,
                LastActive = DateTime.Now
            };
            context.ProjectUsers.Add(projectUser);
            return projectUser;
        }

        public ProjectUser GetProjectUser(string username, Project project)
        {
            if (project.ProjectUsers == null)
            {
                return null;
            }
            return project.ProjectUsers.Where(pu => pu.Username == username).SingleOrDefault();
        }

        public ProjectUser GetProjectUser(int id)
        {
            return context.ProjectUsers.Where(pu => pu.Id == id).SingleOrDefault();
        }

        public void UpdateLastActive(int id, DateTime? date = null)
        {
            var lastActive = date;
            if (lastActive == null)
            {
                lastActive = DateTime.Now;
            }
            context.ProjectUsers.Where(pu => pu.Id == id).SingleOrDefault().LastActive = lastActive.Value;
        }

        public List<ProjectUser> GetProjectUsers(int pid)
        {
            return context.ProjectUsers.Where(pu => pu.ProjectId == pid).ToList();
        }        

        public double GetUsage(int projectUserId, DateTime fromDate, ISessionRepository sessionRepository, IEventRepository eventRepository)
        {
            var projectUser = GetProjectUser(projectUserId);

            var sessions = sessionRepository.GetSessionsForProject(projectUser.ProjectId, projectUserId);
            double totalUsage = 0;
            foreach (var session in sessions)
            {
                List<Event> events = eventRepository.GetEventsFor(session).Where(e => e.Date > fromDate).OrderBy(e => e.Date).ToList();
                if (events.Count > 0)
                {
                    totalUsage += (events[events.Count - 1].Date - events[0].Date).TotalSeconds;
                }
            }
            return totalUsage;
        }
    }
}
