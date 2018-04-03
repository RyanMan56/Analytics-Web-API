using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IProjectRepository
    {
        bool Create(string name, string password, List<int> analysers, string url);
        Project GetProject(int id, bool withAnalysers);
        Project GetProjectByApiKey(string apiKey, bool withAnalysers = false);
        List<Analyser> GetAnalysersForProject(int id);
        bool IsUserAnalyserOfProject(int uid, Project project);
        bool ValidateProjectPassword(string password, Project project);
        bool Save();
    }
}
