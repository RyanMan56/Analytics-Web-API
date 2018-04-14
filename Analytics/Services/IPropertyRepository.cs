using Analytics.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IPropertyRepository
    {
        List<Property> GetPropertiesForEvent(int eventId);
    }
}
