using Microsoft.EntityFrameworkCore;
using RotoEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace RotoTools
{
    public class MyDbContext : DbContext
    {
        public DbSet<FittingGroup> FittingGroups { get; set; }
        public DbSet<Fitting> Fittings { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Option> Options { get; set; }
        public DbSet<Operation> Operations { get; set; }
        public DbSet<Set> Sets { get; set; }
        public DbSet<Opening> Openings { get; set; }
        public DbSet<SetDescription> SetDescriptions { get; set; }
        public DbSet<Colour> Colours { get; set; }


        private readonly string _connectionString;

        public MyDbContext(string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlServer(_connectionString);
        }

        protected override void OnModelCreating(ModelBuilder model)
        {
            foreach (var entityType in model.Model.GetEntityTypes())
            {
                var idProperty = entityType.FindProperty("Id");
                if (idProperty != null)
                {
                    idProperty.ValueGenerated = Microsoft.EntityFrameworkCore.Metadata.ValueGenerated.Never;
                }
            }
        }
    }
}
