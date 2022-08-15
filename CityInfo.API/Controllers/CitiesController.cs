using CityInfo.API.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections;

namespace CityInfo.API.Controllers
{
    [ApiController]
    [Route("api/cities")]
    public class CitiesController : ControllerBase
    {
        [HttpGet]
        public ActionResult<IEnumerable> GetCities()
        {
            return Ok(CitiesDataStore.Current);
        }

        [HttpGet("{id}")]
        public ActionResult<CityDto> GetCity(int id)
        {
            var city = CitiesDataStore.Current.Cities.FirstOrDefault(c => c.Id == id);
            
            if(city == null)
            {
                return NotFound();
            }

            return Ok(city);
        }
    }
}
