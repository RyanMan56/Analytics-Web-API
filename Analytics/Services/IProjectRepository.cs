using Analytics.Entities;
using Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IProjectRepository
    {
        Project Create(string name, string analyserName, string password, List<int> analysers, string url);
        Session CreateSession(int projectId, int projectUserId);
        List<Project> GetProjects(int userId, bool withAnalysers);
        Project GetProject(int id, bool withAnalysers);
        Project GetProjectByApiKey(string apiKey, bool withAnalysers = false);
        Analyser AddAnalyserToProject(string username, Project project);
        List<Analyser> GetAnalysersForProject(int id);
        bool IsAnalyserOfProject(int uid, Project project);
        bool IsProjectUserOfProject(int uid, Project project);
        bool ValidateProjectPassword(string password, Project project);
        List<ProjectUser> GetProjectUsersOfProject(Project project);
        Metric AddMetric(int projectId, MetricDto metricDto);
        bool RemoveAnalyserFromProject(int id, int analyserId);
        bool Save();
    }
}
