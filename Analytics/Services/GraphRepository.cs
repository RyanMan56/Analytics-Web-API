using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;
using Analytics.Models.Graph;

namespace Analytics.Services
{
    public class GraphRepository : BaseRepository, IGraphRepository
    {
        public GraphRepository(AnalyticsContext context)
        {
            this.context = context;
        }

        public Graph Create(GraphForCreationDto graphDto, Project project)
        {
            var metric = context.Metrics.Where(m => m.ProjectId == project.Id && m.Name == graphDto.MetricName).SingleOrDefault();
            if (metric == null)
            {
                return null;
            }
            var graph = new Graph
            {
                Title = graphDto.Title,
                Project = project,
                Metric = metric
            };
            context.Graphs.Add(graph);
            return graph;
        }

        public GraphDto GetGraph(int id)
        {
            var graph = context.Graphs.Where(g => g.Id == id).SingleOrDefault();
            if (graph.MetricId == null)
            {
                return null;
            }
            return new GraphDto
            {
                Id = graph.Id,
                Title = graph.Title,
                MetricId = graph.MetricId.Value
            };
        }

        public List<GraphDto> GetGraphsForProject(int projectId)
        {
            var graphs = context.Graphs.Where(g => g.ProjectId == projectId && g.MetricId != null).ToList();
            return AutoMapper.Mapper.Map<List<GraphDto>>(graphs);
        }

        public void DeleteGraph(int id)
        {
            var graph = context.Graphs.Where(g => g.Id == id).SingleOrDefault();
            if (graph == null)
            {
                return;
            }
            context.Graphs.Remove(graph);
        }
    }
}
