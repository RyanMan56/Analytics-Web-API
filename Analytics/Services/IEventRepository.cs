using Analytics.Entities;
using Analytics.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Services
{
    public interface IEventRepository
    {
        Event AddEvent(EventForCreationDto e, int puid, int pid);
        bool Save();
    }
}
