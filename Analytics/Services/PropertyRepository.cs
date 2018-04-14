using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Analytics.Entities;

namespace Analytics.Services
{
    public class PropertyRepository : BaseRepository, IPropertyRepository
    {
        public PropertyRepository(AnalyticsContext context)
        {
            this.context = context;
        }

        public List<Property> GetPropertiesForEvent(int eventId)
        {
            return context.Properties.Where(p => p.EventId == eventId).ToList();
        }
    }
}
