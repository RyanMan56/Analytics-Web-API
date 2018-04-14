using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;

namespace Analytics.Services
{
    public class ProjectUserRepository : BaseRepository, IProjectUserRepository
    {
        private ISessionRepository sessionRepository;

        public ProjectUserRepository(AnalyticsContext context, ISessionRepository sessionRepository)
        {
            this.context = context;
            this.sessionRepository = sessionRepository;
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

        public List<ProjectUser> GetProjectUsers(int pid)
        {
            return context.ProjectUsers.Where(pu => pu.ProjectId == pid).ToList();
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
    }
}
