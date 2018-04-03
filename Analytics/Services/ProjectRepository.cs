using System.Collections.Generic;
using System.Linq;
using Analytics.Entities;
using Analytics.Utils;
using SimpleCrypto;

namespace Analytics.Services
{
    public class ProjectRepository : BaseRepository, IProjectRepository
    {
        private ICryptoService cryptoService;
        private IProjectUserRepository projectUserRepository;

        public ProjectRepository(AnalyticsContext context, IProjectUserRepository projectUserRepository) {
            this.context = context;
            this.projectUserRepository = projectUserRepository;
            cryptoService = new PBKDF2();
        }        

        // analyserIds is Analysers' uids
        public bool Create(string name, string password, List<int> analyserIds, string url)
        {
            // Get list of users
            var users = (from first in analyserIds
                            join second in context.Users
                            on first equals second.Id
                            select second).ToList();            

            if (users.Count < 1)
            {
                return false;
            }

            List<Analyser> analysers = new List<Analyser>();
            foreach (var user in users)
            {

                var analyser = new Analyser
                {
                    Username = user.Username,
                    Name = name,
                    UserId = user.Id
                };               
                analysers.Add(analyser);                
            }
            context.Analysers.AddRange(analysers);

            var project = new Project
            {
                Name = name,
                Password = cryptoService.Compute(password),
                PasswordSalt = cryptoService.Salt,
                Analysers = analysers,
                Url = url,
                ApiKey = KeyGen.Generate()
            };            

            context.Projects.Add(project);
            return true;
        }

        public Project GetProject(int id, bool withAnalysers)
        {
            var project = context.Projects.Where(p => p.Id == id).SingleOrDefault();
            if (!withAnalysers)
            {
                return project;
            }
            var analysers = GetAnalysersForProject(id);
            var projectUsers = projectUserRepository.GetProjectUsers(id);
            if (analysers.Count < 1)
            {
                return project;
            }
            project.Analysers = analysers;
            project.ProjectUsers = projectUsers;
            return project;
        }
            
        public Project GetProjectByApiKey(string apiKey, bool withAnalysers = false)
        {
            var project = context.Projects.Where(p => p.ApiKey == apiKey).SingleOrDefault();
            if (withAnalysers)
            {
                project.Analysers = GetAnalysersForProject(project.Id);
            }
            project.ProjectUsers = projectUserRepository.GetProjectUsers(project.Id);
            return project;
        }

        public bool ValidateProjectPassword(string password, Project project)
        {
            string hashed = cryptoService.Compute(password, project.PasswordSalt);
            if (!hashed.Equals(project.Password))
            {
                return false;
            }
            return true;
        }

        public List<Analyser> GetAnalysersForProject(int id)
        {
            return context.Analysers.Where(a => a.ProjectId == id).ToList();
        }

        public bool IsUserAnalyserOfProject(int uid, Project project)
        {
            return project.Analysers.Where(a => a.UserId == uid).Any();
        }
    }
}
