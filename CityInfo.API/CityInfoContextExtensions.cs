using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;

namespace CityInfo.API
{
    public static class CityInfoContextExtensions
    {
        // this keyword tells compiler that the class extends CityInfoContext
        public static void EnsureSeedDataForContext(this CityInfoContext context)
        {
            // No need to seed data if there is at least a city existing
            if (context.Cities.Any())
            {
                return;
            }

            // init seed data
            var cities = new List<City>()
            {
                new City()
                {
                    Name = "New York City",
                    Description = "Some description for New York City",
                    PointsOfInterest =  new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "POI 1 - New York City",
                            Description = "POI 1 description"
                        },
                        new PointOfInterest()
                        {
                            Name = "POI 4 - New York City",
                            Description = "POI 4 description"
                        }
                    }
                },
                new City()
                {
                    Name = "Antwerp",
                    Description = "Some description for Antwerp",
                    PointsOfInterest =  new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "POI 2 - Antwerp",
                            Description = "POI 2 description"
                        }
                    }
                },
                new City()
                {
                    Name = "Paris",
                    Description = "Some description for Paris",
                    PointsOfInterest =  new List<PointOfInterest>()
                    {
                        new PointOfInterest()
                        {
                            Name = "POI 3 - Paris",
                            Description = "POI 3 description"
                        }
                    }
                }
            };

            // add data to context
            context.Cities.AddRange(cities);

            // insert data into db
            context.SaveChanges();
        }
    }
}
