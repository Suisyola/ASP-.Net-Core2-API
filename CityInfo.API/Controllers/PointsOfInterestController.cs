using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
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
        private ICityInfoRepository _cityInfoRepository;

        public PointsOfInterestController(ILogger<PointsOfInterestController> logger,
            IMailService mailService, ICityInfoRepository cityInfoRepository)
        {
            _logger = logger;
            _mailService = mailService;
            _cityInfoRepository = cityInfoRepository;
        }

        // GET api/cities/1/pointsOfInterest
        [HttpGet("{cityId}/pointsOfInterest")] 
        public IActionResult GetPointsOfInterest(int cityId)
        {
            try
            {
                if (!_cityInfoRepository.CityExists(cityId))
                {
                    //    // The param in .LogInformation() makes use of String Interpolation. It replaces {cityId} with value.
                    _logger.LogInformation($"City with id {cityId} wasn't found when accessing points of interest");
                    return NotFound();
                }

                var pointsOfInterestForCity = _cityInfoRepository.GetPointOfInterestForCity(cityId);

                var pointsOfInterestForCityResults =
                    Mapper.Map<IEnumerable<PointOfInterestDto>>(pointsOfInterestForCity);

                return Ok(pointsOfInterestForCityResults);
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
        public IActionResult GetPointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterest = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterest == null)
            {
                return NotFound();
            }

            var pointOfInterestResult = Mapper.Map<PointOfInterestDto>(pointOfInterest);

            return Ok(pointOfInterestResult);
        }

        // POST api/cities/1/pointsOfInterest
        // Name attribute, GetPointOfInterest, is refered in the return statement.
        [HttpPost("{cityId}/pointsofinterest", Name = "GetPointOfInterest")]
        public IActionResult CreatePointOfInterest(int cityId,
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
            
            // If the city cannot be found
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = Mapper.Map<Entities.PointOfInterest>(pointOfInterest);

            _cityInfoRepository.AddPointOfInterestForCity(cityId, finalPointOfInterest);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            var createdPointOfInterestToReturn = Mapper.Map<Models.PointOfInterestDto>(finalPointOfInterest);
            
            // This will populate Response header, location, http://localhost:1028/api/cities/3/pointsOfInterest?id=5
            return CreatedAtRoute("GetPointOfInterest",
                new { cityId = cityId, id = createdPointOfInterestToReturn.Id}, createdPointOfInterestToReturn);
        }

        // PUT api/cities/1/pointsOfInterest/2
        // HTTP standard indicates that PUT should fully update resource. Hence, API consumer should provide all values for all fields of the resource, except the ID.
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
            
            
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            // AutoMapper to override values in the 2nd param from 1st param
            // Have to use this overload rather than Mapper.Map<Entities.PointOfInterest>(pointOfInterest), because Entity Framework tracks pointOfInterestEntity.
            // Using this overload will result in pointOfInterestEntity having a Modified state, and then be updated back into database.
            Mapper.Map(pointOfInterest, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

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
            
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            // Map a PointOfInterestForUpdateDto object from the domain object to patch
            var pointOfInterestToPatch = Mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);
            
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

            Mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            return NoContent();
        }

        [HttpDelete("{cityId}/pointsofinterest/{id}")]
        public IActionResult DeletePointOfInterest(int cityId, int id)
        {
            if (!_cityInfoRepository.CityExists(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = _cityInfoRepository.GetPointOfInterestForCity(cityId, id);
            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityInfoRepository.DeletePointOfInterest(pointOfInterestEntity);

            if (!_cityInfoRepository.Save())
            {
                return StatusCode(500, "A problem happened while handling your request.");
            }

            // call mail service to send email
            _mailService.Send("Point of interest deleted.", $"Point of interest {pointOfInterestEntity.Name} " +
                                                            $"with id {pointOfInterestEntity.Id} was deleted.");

            return NoContent();
        }
    }
}