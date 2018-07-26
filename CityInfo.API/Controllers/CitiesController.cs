using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")] // Prefix "api/cities" to all methods.
    //[Route("api/[controller]")] // Alternate way to prefix "api/cities" to all methods. [controller] will be replaced by the prefix of CitiesController (i.e. cities). Not recommended as the URI will change when name of class change.
    public class CitiesController : Controller
    {
        [HttpGet()]
        public IActionResult GetCities()
        {
            return Ok(CitiesDataStore.Current.Cities);
        }

        [HttpGet("{id}")]  // curly brackets used when there is parameters. id in URI will be mapped to int id of action method.
        public IActionResult GetCity(int id)
        {
            // find cities
            var cityToReturn = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            if (cityToReturn == null)
            {
                return NotFound();
            }

            // Return HTTP 200, with the city object in the response body.
            return Ok(cityToReturn);
            
        }
    }
}
