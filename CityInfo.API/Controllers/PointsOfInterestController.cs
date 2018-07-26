using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace CityInfo.API.Controllers
{
    [Route("api/cities")]
    public class PointsOfInterestController : Controller
    {
        private ILogger<PointsOfInterestController> _logger;
        private IMailService _mailService;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService)
        {
            _logger = logger;
            _mailService = mailService;
        }

        // GET api/cities/1/pointsOfInterest
        [HttpGet("{cityId}/pointsOfInterest")] 
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    // The param in .LogInformation() makes use of String Interpolation. It replaces {cityId} with value.
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }

                return Ok(city.PointOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting points of interest for city with id {cityId}.", ex);

                // This error message will return to API consumer, along with HTTP 500. Recommended not to pass stack trace.
                return StatusCode(500, "A problem happened while handling your request.");
            }
            
        }

        // GET api/cities/1/pointsOfInterest/2
        [HttpGet("{cityId}/pointsOfInterest/{id}")]
        public IActionResult GetPointsOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterest = city.PointOfInterest.SingleOrDefault(p => p.Id == id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterest);
        }

        // POST api/cities/1/pointsOfInterest
        // Name attribute, GetPointOfInterest, is refered in the return statement.
        [HttpPost("{cityId}/pointsofinterest", Name = "GetPointOfInterest")]
        public IActionResult CreationPointOfInterest(int cityId,
            [FromBody] PointOfInterestForCreationDto pointOfInterest)
        {
            // If cannot deserialise data from Request body into PointOfInterestDtoForCreation
            if (pointOfInterest == null)
            {
                return BadRequest();
            }

            // Customised validation logic
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                // Adding error message for customised validation logic
                ModelState.AddModelError("Description", "Name and Description cannot be the same");
            }

            // Check again Data Annotation for PointOfInterestForCreationDto, if the model is not valid
            if (!ModelState.IsValid)
            {
                // If there are ErrorMessage set in DTO, need to pass back ModelState. This is to display the ErrorMessage
                return BadRequest(ModelState);
            }

            // Get the city to create the point of interest for
            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

            // If the city cannot be found
            if (city == null)
            {
                return NotFound();
            }

            var maxPointOfInterestId = CitiesDataStore.Current.Cities.SelectMany(c => c.PointOfInterest).Max(p => p.Id);

            var finalPointOfInterest = new PointOfInterestDto()
            {
                Id = ++maxPointOfInterestId,
                Name = pointOfInterest.Name,
                Description = pointOfInterest.Description
            };

            city.PointOfInterest.Add(finalPointOfInterest);

            // This will populate Response header, location, http://localhost:1028/api/cities/3/pointsOfInterest?id=5
            return CreatedAtRoute("GetPointOfInterest",
                new { cityId = cityId, id = finalPointOfInterest.Id}, finalPointOfInterest);
        }

        // PUT api/cities/1/pointsOfInterest/2
        [HttpPut("{cityId}/pointsofinterest/{id}")]
        public IActionResult UpdatePointOfInterest(int cityId, int id,
            [FromBody] PointOfInterestForUpdateDto pointOfInterest)
        {
            if (pointOfInterest == null)
            {
                return BadRequest();
            }
            
            if (pointOfInterest.Description == pointOfInterest.Name)
            {
                ModelState.AddModelError("Description", "Name and Description cannot be the same");
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);
            
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            // HTTP standard indicates that PUT should fully update resource. Hence, API consumer should provide all values for all fields of the resource, except the ID.
            pointOfInterestFromStore.Name = pointOfInterest.Name;
            pointOfInterestFromStore.Description = pointOfInterest.Description;

            // Return 204 - No Content. No need to send info about the updated object back to consumer, as they already had the info
            return NoContent();
        }

        [HttpPatch("{cityId}/pointsofinterest/{id}")]
        public IActionResult PartiallyUpdatePointOfInterest(int cityId, int id,
            [FromBody] JsonPatchDocument<PointOfInterestForUpdateDto> patchDoc)
        {
            if (patchDoc == null)
            {
                return BadRequest();
            }

            var city = CitiesDataStore.Current.Cities.SingleOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            // Create a PointOfInterestForUpdateDto object from the domain object to patch
            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestFromStore.Name,
                Description =  pointOfInterestFromStore.Description
            };

            // Need to pass in Modelstate, so that any validation error flagged from
            // data annotation would be added to ModelState's error collection
            // However, this validates JsonPatchDocument(patchDoc) and not pointOfInterestToPatch
            patchDoc.ApplyTo(pointOfInterestToPatch, ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (pointOfInterestToPatch.Description == pointOfInterestToPatch.Name)
            {
                ModelState.AddModelError("Description", "Name and Description cannot be the same");
            }
            
            // Validate the object that was patched
            TryValidateModel(pointOfInterestToPatch);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Update domain class if pass validation
            pointOfInterestFromStore.Name = pointOfInterestToPatch.Name;
            pointOfInterestFromStore.Description = pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{cityId}/pointsofinterest/{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestFromStore = city.PointOfInterest.FirstOrDefault(p => p.Id == id);

            if (pointOfInterestFromStore == null)
            {
                return NotFound();
            }

            city.PointOfInterest.Remove(pointOfInterestFromStore);

            // call mail service to send email
            _mailService.Send("Point of interest deleted.", $"Point of interest {pointOfInterestFromStore.Name} " +
                                                            $"with id {pointOfInterestFromStore.Id} was deleted.");

            return NoContent();
        }
    }
}