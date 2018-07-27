using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")] // Prefix "api/cities" to all methods.
    //[Route("api/[controller]")] // Alternate way to prefix "api/cities" to all methods. [controller] will be replaced by the prefix of CitiesController (i.e. cities). Not recommended as the URI will change when name of class change.
    public class CitiesController : Controller
    {
        private ICityInfoRepository _cityInfoRepository;

        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }

        [HttpGet()]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();

            // Indicate in <> the type we want to Get back, i.e. IEnumerable<CityWithoutPointOfInterestDto> type, and pass in entity fetched from repository
            var results = Mapper.Map<IEnumerable<CityWithoutPointOfInterestDto>>(cityEntities);

            return Ok(results);
        }

        // GET api/cities/1?includePointsOfInterest=true
        [HttpGet("{id}")]  // curly brackets used when there is parameters. id in URI will be mapped to int id of action method.
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);

            if (city == null)
            {
                return NotFound();
            }
            
            // returns DTO with city and it's points of interest
            if (includePointsOfInterest)
            {
                var cityResult = Mapper.Map<CityDto>(city);

                return Ok(cityResult);
            }

            var cityWithoutPointsOfInterestResult = Mapper.Map<CityWithoutPointOfInterestDto>(city);

            return Ok(cityWithoutPointsOfInterestResult);

        }
    }
}
