using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Analytics.Entities
{
    public class AnalyticsContext : DbContext
    {
        public AnalyticsContext(DbContextOptions<AnalyticsContext> options) : base(options)
        {
            Database.Migrate();
        }

        // DbSet holds values of a table from the db. LINQ Queries made against DbSet are translated into sql and performed on the db
        public DbSet<User> Users { get; set; }
    }
}
