using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public class AnalyticsRepository : IAnalyticsRepository
    {
        private AnalyticsContext context;

        public AnalyticsRepository(AnalyticsContext context)
        {
            this.context = context;
        }
    }
}
