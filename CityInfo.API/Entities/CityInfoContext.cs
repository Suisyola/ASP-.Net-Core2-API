using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Entities
{
    public class CityInfoContext : DbContext
    {
        public CityInfoContext(DbContextOptions<CityInfoContext> options)
            : base(options)
        {
            // If DB do not exists, create it. If DB exists, nothing happens
            Database.Migrate();
        }

        public DbSet<City> Cities { get; set; }

        public DbSet<PointOfInterest> PointOfInterests { get; set; }

        /*
        // One of the 2 methods to using SQL Server with connection string
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("connectionString");

            base.OnConfiguring(optionsBuilder);
        }
        */
    }
}
