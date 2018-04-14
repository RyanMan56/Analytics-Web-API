using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IProjectRepository
    {
        Project Create(string name, string password, List<int> analysers, string url);
        Session CreateSession(int projectId, int projectUserId);
        Project GetProject(int id, bool withAnalysers);
        Project GetProjectByApiKey(string apiKey, bool withAnalysers = false);
        List<Analyser> GetAnalysersForProject(int id);
        bool IsAnalyserOfProject(int uid, Project project);
        bool IsProjectUserOfProject(int uid, Project project);
        bool ValidateProjectPassword(string password, Project project);
        bool Save();
    }
}
