using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IProjectUserRepository
    {
        ProjectUser CreateProjectUser(string username, Project project);
        ProjectUser GetProjectUser(string username, Project project);
        ProjectUser GetProjectUser(int id);
        List<ProjectUser> GetProjectUsers(int pid);
        void UpdateLastActive(int id, DateTime? date = null);
        bool Save();
    }
}
