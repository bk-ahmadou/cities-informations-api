using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace CityInfo.API.Controllers
{
    [Route("api/cities/{cityId}/pointsOfinterest")]
    [ApiController]
    public class PointsOfInterestController : ControllerBase
    {

        private readonly ILogger<PointsOfInterestController> _logger;
        private readonly IMailService _mailService;
        private readonly CitiesDataStore _cityDataStore; 
        
        public PointsOfInterestController(CitiesDataStore citiesDataStore,IMailService mailService,ILogger<PointsOfInterestController> logger)
        {
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService)); ;
            _cityDataStore = citiesDataStore ?? throw new ArgumentNullException(nameof(citiesDataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable<PointsOfInterestDto>> GetPointsOfInterest(int cityId)
        {
            try
            {
                var city = _cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

                if (city == null)
                {
                    _logger.LogInformation($"City with id {cityId} was not found when accessing points of interest.");
                    return NotFound();
                }

                return Ok(city.PointsOfInterest);
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting point of interest for city with id {cityId}.",ex);

                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("{pointofinterestid}", Name ="GetPointOfInterest")]
        public ActionResult<PointsOfInterestDto> GetPointOfInterest(int cityId, int pointofinterestid)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);

            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterst=city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);
            
            if(pointOfInterst == null)
            {
                return NotFound();
            }

            return Ok(pointOfInterst);
        }

        [HttpPost]
        public ActionResult<PointsOfInterestDto> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            var city=_cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var maxPointOfInterestid = _cityDataStore.Cities.SelectMany(c => c.PointsOfInterest).Max(p => p.Id);

            var finalPointOfInterest = new PointsOfInterestDto()
            {
                Id = ++maxPointOfInterestid,
                Name=pointOfInterest.Name,
                Description=pointOfInterest.Description
            };

            city.PointsOfInterest.Add(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId=cityId,
                    pointOfInterestId=finalPointOfInterest.Id
                },
                finalPointOfInterest);
        }

        [HttpPut("{pointofinterestid}")]
        public ActionResult UpdatePointOfInterest(int cityId, int pointofinterestid, PointOfInterestForUpdateDto pointOfInterest)
        {
            var city=_cityDataStore.Cities.FirstOrDefault(c=>c.Id==cityId);

            if(city == null)
            {
                return NotFound();
            }

            var pointOfInterestToUpdate=city.PointsOfInterest.FirstOrDefault(p => p.Id==pointofinterestid);

            if(pointOfInterestToUpdate == null)
            {
                return NotFound();
            }

            pointOfInterestToUpdate.Name=pointOfInterest.Name;
            pointOfInterestToUpdate.Description=pointOfInterest.Description;

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public ActionResult PartiallyUpdatePointOfInterest(
            int cityId, int pointofinterestid,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestToUpdate = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);

            if (pointOfInterestToUpdate == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = new PointOfInterestForUpdateDto()
            {
                Name = pointOfInterestToUpdate.Name,
                Description = pointOfInterestToUpdate.Description,
            };

            patchDocument.ApplyTo(pointOfInterestToPatch,ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            pointOfInterestToUpdate.Name= pointOfInterestToPatch.Name;
            pointOfInterestToUpdate.Description=pointOfInterestToPatch.Description;

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public IActionResult DeletePintOfInterest(int cityId, int pointofinterestid)
        {
            var city = _cityDataStore.Cities.FirstOrDefault(c => c.Id == cityId);
            if (city == null)
            {
                return NotFound();
            }

            var pointOfInterestToDelete = city.PointsOfInterest.FirstOrDefault(p => p.Id == pointofinterestid);

            if (pointOfInterestToDelete == null)
            {
                return NotFound();
            }

            city.PointsOfInterest.Remove(pointOfInterestToDelete);
            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestToDelete.Name} with id {pointofinterestid} was deleted.");
            return NoContent();
        }
    }
}
