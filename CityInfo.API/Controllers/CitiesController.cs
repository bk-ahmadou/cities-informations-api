using AutoMapper;
using CityInfo.API.Models;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections;
using System.Text.Json;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {

        private readonly IMapper _mapper;
        private readonly ICityRepository _cityInfoRepository;
        const int maxCitiesPageSize = 20;

        public CitiesController(IMapper mapper, ICityRepository cityInfoRepository)
        {
            _mapper = mapper?? throw new ArgumentNullException(nameof(mapper));
            _cityInfoRepository = cityInfoRepository ?? throw new ArgumentNullException(nameof(cityInfoRepository));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CityWithoutPointOfInterestDto>>> GetCities(string? name, string? searchQuery, int pageNumber=1, int pageSize=10)
        {
            if(pageSize> maxCitiesPageSize)
            {
                pageSize = maxCitiesPageSize;
            }
            var (cities, paginationMetaData) = await _cityInfoRepository.GetCitiesAsync(name,searchQuery,pageNumber,pageSize);

            Response.Headers.Add("X-Paggination", JsonSerializer.Serialize(paginationMetaData));

            return Ok(_mapper.Map<IEnumerable<CityWithoutPointOfInterestDto>>(cities));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCity(int id, bool includePointOfInterest=false)
        {
            var city = await _cityInfoRepository.GetCityAsync(id, includePointOfInterest);
            
            if(city == null)
            {
                return NotFound();
            }

            if (includePointOfInterest)
            {
                return Ok(_mapper.Map<CityDto>(city));
            }

            return Ok(_mapper.Map<CityWithoutPointOfInterestDto>(city));
        }
    }
}
