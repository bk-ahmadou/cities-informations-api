using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {

        private readonly CitiesDataStore _dataStore;

        public CitiesController(CitiesDataStore dataStore)
        {
            _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
        }

        [HttpGet]
        public ActionResult<IEnumerable> GetCities()
        {
            return Ok(_dataStore.Cities);
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var city = _dataStore.Cities.FirstOrDefault(c => c.Id == id);
            
            if(city == null)
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}
