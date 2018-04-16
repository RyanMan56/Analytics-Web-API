using System.Collections.Generic;
using System.Linq;
using Analytics.Entities;
using Analytics.Utils;
using SimpleCrypto;
using Microsoft.EntityFrameworkCore;
using Analytics.Models;

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
        public Project Create(string name, string analyserName, string password, List<int> analyserIds, string url)
        {
            // Get list of users
            var users = (from first in analyserIds
                         join second in context.Users
                         on first equals second.Id
                         select second).ToList();

            if (users.Count < 1)
            {
                return null;
            }

            var finalUsers = new List<User>();
            var preExistingAnalysers = new List<Analyser>();
            foreach (var user in users)
            {
                var analyser = context.Analysers.Where(a => a.UserId == user.Id).SingleOrDefault();
                if (analyser != null)
                {
                    // Then analyser already exists so no need to create
                    preExistingAnalysers.Add(analyser);
                }
                else
                {
                    finalUsers.Add(user);
                }

            }

            List<Analyser> analysers = new List<Analyser>();            
            foreach (var user in finalUsers)
            {

                var analyser = new Analyser
                {
                    Username = user.Username,
                    Name = analyserName,
                    UserId = user.Id,                    
                };
                analysers.Add(analyser);
            }
            context.Analysers.AddRange(analysers);            

            var project = new Project
            {
                Name = name,
                Password = cryptoService.Compute(password),
                PasswordSalt = cryptoService.Salt,
                Url = url,
                ApiKey = KeyGen.Generate()
            };

            if (!Save())
            {
                return null;
            }

            if (preExistingAnalysers != null)
            {
                analysers.AddRange(preExistingAnalysers);
            }

            foreach (var analyser in analysers)
            {
                context.Analysers.Where(a => a.Id == analyser.Id).Select(a => a.ProjectAnalysers).SingleOrDefault()
                    .Add(new ProjectAnalyser
                        {
                            Analyser = analyser,
                            Project = project
                        });
            }

            context.Projects.Add(project);
            return project;
        }

        public Session CreateSession(int projectId, int projectUserId)
        {
            var session = new Session
            {
                ProjectId = projectId,
                ProjectUserId = projectUserId
            };

            context.Sessions.Add(session);
            return session;
        }

        public List<Project> GetProjects(int userId, bool withAnalysers)
        {
            var projects = context.Projects.Where(p => p.ProjectAnalysers.Where(pa => pa.Analyser.UserId == userId).Any()).ToList();
            if (!withAnalysers)
            {
                return projects;
            }
            foreach (var project in projects)
            {
                var projectAnalysers = GetProjectAnalysersForProject(project.Id);
                var projectUsers = projectUserRepository.GetProjectUsers(project.Id);

                project.ProjectUsers = projectUsers;

                project.ProjectAnalysers = projectAnalysers;
            }
            return projects;
        }

        public Project GetProject(int id, bool withAnalysers)
        {
            var project = context.Projects.Where(p => p.Id == id).SingleOrDefault();
            if (project == null)
            {
                return null;
            }
            if (!withAnalysers)
            {
                return project;
            }
            var projectAnalysers = GetProjectAnalysersForProject(id);
            var projectUsers = projectUserRepository.GetProjectUsers(id);

            project.ProjectUsers = projectUsers;

            project.ProjectAnalysers = projectAnalysers;
            return project;
        }
            
        public Project GetProjectByApiKey(string apiKey, bool withAnalysers = false)
        {
            var project = context.Projects.Where(p => p.ApiKey == apiKey).SingleOrDefault();            
            if (project == null)
            {
                return null;
            }
            if (withAnalysers)
            {
                project.ProjectAnalysers = GetProjectAnalysersForProject(project.Id);
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

        public Analyser AddAnalyserToProject(string username, Project project)
        {
            var finalAnalyser = context.Analysers.Include(a => a.ProjectAnalysers)
                .Where(a => a.Username == username).SingleOrDefault();

            if (finalAnalyser != null)
            {
                // Check if analyser is already on the project. If so return null                
                if (finalAnalyser.ProjectAnalysers.Where(pa => pa.ProjectId == project.Id).Any())
                {
                    return null;
                }                
            }
            
            if (finalAnalyser == null)
            {
                var user = context.Users.Where(u => u.Username == username).SingleOrDefault();
                if (user == null)
                {
                    return null;
                }
                var analyser = new Analyser
                {
                    Name = user.Name,
                    User = user,
                    Username = username
                };
                context.Analysers.Add(analyser);
                finalAnalyser = analyser;
            }            
            project.ProjectAnalysers.Add(new ProjectAnalyser
            {
                Project = project,
                Analyser = finalAnalyser
            });
            return finalAnalyser;
        }

        public List<Analyser> GetAnalysersForProject(int id)
        {
            //return context.ProjectAnalysers.Where(pa => pa.ProjectId == id).Select(pa => pa.Analyser).ToList();

            var analysers = context.Analysers.Include(a => a.ProjectAnalysers)
                    .ThenInclude(pa => pa.Project)
                    .ToList();

            return analysers.Where(a => a.ProjectAnalysers.Where(pa => pa.ProjectId == id).Any()).ToList();
        }
             
        public List<ProjectAnalyser> GetProjectAnalysersForProject(int id)
        {
            //var projectAnalysers = context.Analysers.Include(a => a.ProjectAnalysers)
            //                        .ThenInclude(pa => pa.Project)
            //                        .Select(a => a.ProjectAnalysers.Where(pa => pa.ProjectId == id))

            var project = context.Projects.Include(p => p.ProjectAnalysers)
                                    .ThenInclude(pa => pa.Analyser)
                                    .Where(p => p.Id == id).SingleOrDefault();

            if (project == null)
            {
                return null;
            }

            for (var i = 0; i < project.ProjectAnalysers.Count(); i++)
            {
                project.ProjectAnalysers[i].AnalyserId = project.ProjectAnalysers[i].Analyser.Id;
                project.ProjectAnalysers[i].ProjectId = project.Id;
            }
            return project.ProjectAnalysers;
            
        }

        public bool IsAnalyserOfProject(int uid, Project project)
        {
            //return context.ProjectAnalysers.Where(pa => pa.Analyser.UserId == uid).Any();
            return project.ProjectAnalysers.Where(pa => pa.Analyser.UserId == uid).Any();
        }

        public bool IsProjectUserOfProject(int uid, Project project)
        {
            return project.ProjectUsers.Where(u => u.Id == uid).Any();
        }

        public List<ProjectUser> GetProjectUsersOfProject(Project project)
        {
            return context.ProjectUsers.Where(pu => pu.ProjectId == project.Id).ToList();
        }

        public Metric AddMetric(int projectId, MetricDto metricDto)
        {
            if (context.Metrics.Where(m => m.Name == metricDto.Name).Any())
            {
                // If a metric with the same name already exists, don't add this one
                return null;
            }
            var finalMetricParts = new List<MetricPart>();
            var metric = new Metric
            {
                Name = metricDto.Name,
                MetricType = metricDto.MetricType,
                ProjectId = projectId
            };

            if (!Save())
            {
                return null;
            }

            foreach (var metricPart in metricDto.MetricParts)
            {
                finalMetricParts.Add(new MetricPart
                {
                    EventName = metricPart.EventName,
                    EventProperty = metricPart.EventProperty,
                    MetricId = metric.Id
                });
            }

            metric.MetricsParts = finalMetricParts;

            context.Metrics.Add(metric);
            return metric;
        }
    }
}
