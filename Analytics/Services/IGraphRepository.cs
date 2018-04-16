using Analytics.Entities;
using Analytics.Models.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IGraphRepository
    {
        GraphDto GetGraph(int id);
        List<GraphDto> GetGraphsForProject(int projectId);
        Graph Create(GraphForCreationDto graph, Project project);
        void DeleteGraph(int id);
        bool Save();
    }
}
