using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;

namespace CityInfo.API.Services
{
    /// <summary>
    /// This is the contract that the Repository implementation will have to adhere to
    /// </summary>

    public interface ICityInfoRepository
    {
        IEnumerable<City> GetCities();

        bool CityExists(int cityId);

        City GetCity(int cityId, bool includePointsOfInterest);

        IEnumerable<PointOfInterest> GetPointOfInterestForCity(int cityId);

        PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId);

        void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest);

        bool Save();

        void DeletePointOfInterest(PointOfInterest pointOfInterest);
    }
}
