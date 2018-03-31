using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public class BaseRepository
    {
        protected AnalyticsContext context;

        public bool Save()
        {
            return (context.SaveChanges() >= 0); // SaveChanges returns the number of entities changed
        }
    }
}
