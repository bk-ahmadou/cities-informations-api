using AutoMapper;
using CityInfo.API.Entities;
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
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        
        public PointsOfInterestController(IMapper mapper, ICityRepository cityRepository,IMailService mailService,ILogger<PointsOfInterestController> logger)
        {
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
            _cityRepository=cityRepository?? throw new ArgumentNullException(nameof(cityRepository));
            _logger = logger?? throw new ArgumentNullException(nameof(logger));
            _mailService = mailService ?? throw new ArgumentNullException(nameof(mailService)); ;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PointsOfInterestDto>>> GetPointsOfInterest(int cityId)
        {
            try
            {
                if(!await _cityRepository.CityExistsAsync(cityId))
                {
                    _logger.LogInformation($"City with id {cityId} was not found when accessing points of interest.");
                    return NotFound();
                }
                var pointOfInterestForCity =await _cityRepository.GetPointOfInterestForCityAsync(cityId);

                return Ok(_mapper.Map<IEnumerable<PointsOfInterestDto>>(pointOfInterestForCity));
            }
            catch (Exception ex)
            {
                _logger.LogCritical($"Exception while getting point of interest for city with id {cityId}.",ex);

                return StatusCode(500, "A problem happened while handling your request.");
            }
        }

        [HttpGet("{pointofinterestid}", Name ="GetPointOfInterest")]
        public async Task<ActionResult<PointsOfInterestDto>> GetPointOfInterest(int cityId, int pointofinterestid)
        {
            if(!await _cityRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }
            var pointOfInterest = await _cityRepository.GetPointOfInterestForCityAsync(cityId,pointofinterestid);
            
            if(pointOfInterest == null)
            {
                return NotFound();
            }

            return Ok(_mapper.Map<PointsOfInterestDto>(pointOfInterest));
        }

        [HttpPost]
        public async Task<ActionResult<PointsOfInterestDto>> CreatePointOfInterest(int cityId, PointOfInterestForCreationDto pointOfInterest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest();
            }

            if (!await _cityRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var finalPointOfInterest = _mapper.Map<PointOfInterest>(pointOfInterest);

            await _cityRepository.AddPointOfInterestForCityAsync(cityId,finalPointOfInterest);

            await _cityRepository.SaveChangesAsync() ;

            var createdPointOfInterestToReturn=_mapper.Map<PointsOfInterestDto>(finalPointOfInterest);

            return CreatedAtRoute("GetPointOfInterest",
                new
                {
                    cityId=cityId,
                    pointOfInterestId=createdPointOfInterestToReturn.Id
                },
                createdPointOfInterestToReturn);
        }

        [HttpPut("{pointofinterestid}")]
        public async Task<ActionResult> UpdatePointOfInterest(int cityId, int pointofinterestid, PointOfInterestForUpdateDto pointOfInterest)
        {
            if (!await _cityRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity=await _cityRepository.GetPointOfInterestForCityAsync(cityId,pointofinterestid);

            if(pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _mapper.Map(pointOfInterest, pointOfInterestEntity);

            await _cityRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpPatch("{pointofinterestid}")]
        public async Task<ActionResult> PartiallyUpdatePointOfInterest(
            int cityId, int pointofinterestid,
            JsonPatchDocument<PointOfInterestForUpdateDto> patchDocument)
        {
            if (!await _cityRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestid);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            var pointOfInterestToPatch = _mapper.Map<PointOfInterestForUpdateDto>(pointOfInterestEntity);

            patchDocument.ApplyTo(pointOfInterestToPatch,ModelState);

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (TryValidateModel(pointOfInterestToPatch))
            {
                return BadRequest(ModelState);
            }

            //this results in object2 to contain the changes that were applied to object1
            _mapper.Map(pointOfInterestToPatch, pointOfInterestEntity);

            await _cityRepository.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{pointofinterestid}")]
        public async Task<IActionResult> DeletePintOfInterest(int cityId, int pointofinterestid)
        {
            if (!await _cityRepository.CityExistsAsync(cityId))
            {
                return NotFound();
            }

            var pointOfInterestEntity = await _cityRepository.GetPointOfInterestForCityAsync(cityId, pointofinterestid);

            if (pointOfInterestEntity == null)
            {
                return NotFound();
            }

            _cityRepository.DeletePointOfInterest(pointOfInterestEntity);
            _mailService.Send("Point of interest deleted.",
                $"Point of interest {pointOfInterestEntity.Name} with id {pointofinterestid} was deleted.");
            return NoContent();
        }
    }
}
