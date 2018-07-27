using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;

namespace CityInfo.API.Services
{
    public class CityInfoRepository : ICityInfoRepository
    {
        private CityInfoContext _context;

        public CityInfoRepository(CityInfoContext context)
        {
            _context = context;
        }

        public void AddPointOfInterestForCity(int cityId, PointOfInterest pointOfInterest)
        {
            var city = GetCity(cityId, false);
            city.PointsOfInterest.Add(pointOfInterest);
        }

        public bool Save()
        {
            return (_context.SaveChanges() >= 0);
        }

        public bool CityExists(int cityId)
        {
            return _context.Cities.Any(c => c.Id == cityId);
        }

        public IEnumerable<City> GetCities()
        {

            // need .ToList() in order for Entity Framework to query database at this point of time
            return _context.Cities.OrderBy(c => c.Name).ToList();
        }

        public City GetCity(int cityId, bool includePointsOfInterest)
        {
            if (includePointsOfInterest)
            {
                return _context.Cities
                    // This is to include PointsOfInterest entities that belongs to City in query result
                    .Include(c => c.PointsOfInterest)
                    .Where(c => c.Id == cityId).FirstOrDefault();
            }
            return _context.Cities.Where(c => c.Id == cityId).FirstOrDefault();
        }

        public IEnumerable<PointOfInterest> GetPointOfInterestForCity(int cityId)
        {
            return _context.PointOfInterests.Where(p => p.CityId == cityId).ToList();
        }

        public PointOfInterest GetPointOfInterestForCity(int cityId, int pointOfInterestId)
        {
            return _context.PointOfInterests.Where(p => p.CityId == cityId && p.Id == pointOfInterestId)
                .FirstOrDefault();
        }

        public void DeletePointOfInterest(PointOfInterest pointOfInterest)
        {
            _context.Remove(pointOfInterest);
        }
    }
}
