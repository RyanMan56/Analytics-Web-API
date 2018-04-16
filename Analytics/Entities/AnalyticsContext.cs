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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Configuring the composite key for ProjectAnalyser
            modelBuilder.Entity<ProjectAnalyser>().HasKey(pa => new { pa.ProjectId, pa.AnalyserId });
        }

        // DbSet holds values of a table from the db. LINQ Queries made against DbSet are translated into sql and performed on the db
        public DbSet<User> Users { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Analyser> Analysers { get; set; }
        //public DbSet<ProjectAnalyser> ProjectAnalysers { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Property> Properties { get; set; }
        public DbSet<ProjectUser> ProjectUsers { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Metric> Metrics { get; set; }
        public DbSet<MetricPart> MetricParts { get; set; }
        public DbSet<Graph> Graphs { get; set; }
    }
}
