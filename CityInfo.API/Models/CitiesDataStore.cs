using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CityInfo.API.Models
{
    

    public class CitiesDataStore
    {
        public static CitiesDataStore Current { get; } = new CitiesDataStore();

        public List<CityDto> Cities { get; set; }

        public CitiesDataStore()
        {
            Cities = new List<CityDto>()
            {
                new CityDto()
                {
                    Id = 1,
                    Name = "New York City",
                    Description = "Some description for New York City",
                    PointsOfInterest =  new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 1,
                            Name = "POI 1 - New York City",
                            Description = "POI 1 description"
                        },
                        new PointOfInterestDto()
                        {
                            Id = 4,
                            Name = "POI 4 - New York City",
                            Description = "POI 4 description"
                        }
                    }
                },
                new CityDto()
                {
                    Id = 2,
                    Name = "Antwerp",
                    Description = "Some description for Antwerp",
                    PointsOfInterest =  new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 2,
                            Name = "POI 2 - Antwerp",
                            Description = "POI 2 description"
                        }
                    }
                },
                new CityDto()
                {
                    Id = 3,
                    Name = "Paris",
                    Description = "Some description for Paris",
                    PointsOfInterest =  new List<PointOfInterestDto>()
                    {
                        new PointOfInterestDto()
                        {
                            Id = 3,
                            Name = "POI 3 - Paris",
                            Description = "POI 3 description"
                        }
                    }
                }
            };
        }
    }
}
