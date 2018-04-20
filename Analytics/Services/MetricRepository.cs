using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Analytics.Utils;

namespace Analytics.Services
{
    public class MetricRepository : BaseRepository, IMetricRepository
    {
        public MetricRepository(AnalyticsContext context)
        {
            this.context = context;
        }

        public Metric AddMetricPart(int id, int projectId, MetricPartDto metricPartDto)
        {
            // Get metric
            var metric = context.Metrics.Where(m => m.Id == id && m.ProjectId == projectId).SingleOrDefault();
            // Get metric parts
            metric.MetricsParts = context.MetricParts.Where(mp => mp.MetricId == metric.Id).ToList();
            // Map metricPartDto to MetricPart model
            var metricPart = new MetricPart
            {
                MetricId = metric.Id,
                EventName = metricPartDto.EventName,
                EventProperty = metricPartDto.EventProperty
            };
            metric.MetricsParts.Add(metricPart);
            return metric;
        }

        public bool MetricExists(int id, int projectId)
        {
            return context.Metrics.Where(m => m.Id == id && m.ProjectId == projectId).Any();
        }        

        public Metric GetMetric(int id, int projectId, bool withParts)
        {
            var metric = context.Metrics.Where(m => m.Id == id && m.ProjectId == projectId).SingleOrDefault();
            if (metric == null)
            {
                return null;
            }
            metric.MetricsParts = context.MetricParts.Where(mp => mp.MetricId == id).ToList();
            return metric;
        }

        public void RemoveMetricPart(int id, int projectId, int metricPartId)
        {
            var metric = context.Metrics.Where(m => m.ProjectId == projectId);
            if (metric == null)
            {
                return;
            }
            var metricPart = context.MetricParts.Where(mp => mp.Id == metricPartId).SingleOrDefault();
            context.MetricParts.Remove(metricPart);
        }

        public void RemoveMetric(int id, int projectId)
        {
            var metric = GetMetric(id, projectId, true);
            if (metric == null)
            {
                return;
            }
            context.Metrics.Remove(metric);
        }

        public List<Metric> GetMetrics(int projectId, bool withMetricParts)
        {
            if (!withMetricParts)
            {
                return context.Metrics.Where(m => m.ProjectId == projectId).ToList();
            }
            return context.Metrics.Include(m => m.MetricsParts).Where(m => m.ProjectId == projectId).ToList();
        }

        public double CalculateMetricBeforeDate(Metric metric, DateTime dateLimit)
        {
            if (metric.MetricType == MetricTypes.Accumulative)
            {
                return CalculateAccumulativeMetric(metric, dateLimit);
            }
            if (metric.MetricType == MetricTypes.Average)
            {
                return CalculateAverageMetric(metric, dateLimit);
            }
            if (metric.MetricType == MetricTypes.Sum)
            {
                return CalculateSumMetric(metric, dateLimit);
            }
            return -1;
        }
        
        private double CalculateAccumulativeMetric(Metric metric, DateTime dateLimit)
        {
            int count = 0;
            foreach (var metricPart in metric.MetricsParts)
            {                                
                var sessions = context.Sessions.Include(s => s.Events).ThenInclude(e => e.Properties)
                    .Where(s => s.ProjectId == metric.ProjectId).ToList();
                foreach (var session in sessions)
                {
                    foreach (var e in session.Events)
                    {
                        if (e.Date > dateLimit)
                        {
                            continue;
                        }
                        if (metricPart.EventProperty == "")
                        {
                            if (e.Name == metricPart.EventName)
                            {
                                count++;
                            }
                        }
                        else
                        {
                            var properties = e.Properties.Where(p => p.Name == metricPart.EventProperty).ToList();
                            count += properties.Count();
                        }
                    }
                }
            }
            return count;
        }

        private double CalculateAverageMetric(Metric metric, DateTime dateLimit)
        {
            int count = 0;
            double value = 0;

            foreach (var metricPart in metric.MetricsParts)
            {
                if (metricPart.EventProperty == "")
                {
                    continue; // Skip to the next metricPart
                }
                var sessions = context.Sessions.Include(s => s.Events).ThenInclude(e => e.Properties)
                    .Where(s => s.ProjectId == metric.ProjectId).ToList();
                foreach (var session in sessions)
                {
                    foreach (var e in session.Events)
                    {
                        if (e.Date > dateLimit)
                        {
                            continue;
                        }
                        var properties = e.Properties.Where(p => p.Name == metricPart.EventProperty && p.DataType == DataTypes.Number).ToList();
                        foreach (var prop in properties)
                        {
                            if (e.Name == metricPart.EventName)
                            {
                                value += PropertyParser.ParseNumber(prop.Value);
                                count++;
                            }
                        }                        
                    }
                }
            }
            return value / count;
        }

        private double CalculateSumMetric(Metric metric, DateTime dateLimit)
        {
            double value = 0;
            
            foreach (var metricPart in metric.MetricsParts)
            {
                if (metricPart.EventProperty == "")
                {
                    continue;
                }
                var sessions = context.Sessions.Include(s => s.Events).ThenInclude(e => e.Properties)
                    .Where(s => s.ProjectId == metric.ProjectId).ToList();
                foreach (var session in sessions)
                {
                    foreach (var e in session.Events)
                    {
                        if (e.Date > dateLimit)
                        {
                            continue;
                        }
                        var properties = e.Properties.Where(p => p.Name == metricPart.EventProperty && p.DataType == DataTypes.Number).ToList();
                        foreach (var prop in properties)
                        {
                            if (e.Name == metricPart.EventName)
                            {
                                value += PropertyParser.ParseNumber(prop.Value);
                            }
                        }
                    }
                }
            }
            return value;
        }
    }
}
