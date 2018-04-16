using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IMetricRepository
    {
        Metric AddMetricPart(int id, int projectId, MetricPartDto metricPartDto);
        bool MetricExists(int id, int projectId);
        List<Metric> GetMetrics(int projectId, bool withParts);
        Metric GetMetric(int id, int projectId, bool withParts);
        void RemoveMetricPart(int id, int projectId, int metricPartId);
        void RemoveMetric(int id, int projectId);
        double CalculateMetricBeforeDate(Metric metric, DateTime dateLimit);
        bool Save();
    }
}
