using CityInfo.API.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Models;
using AutoMapper;

namespace CityInfo.API.Controllers
{
    //[Route("/api/[controller]")]
    [Route("/api/cities")]
    public class CitiesController : Controller
    {
        private ICityInfoRepository _cityInfoRepository;
        public CitiesController(ICityInfoRepository cityInfoRepository)
        {
            _cityInfoRepository = cityInfoRepository;
        }

        [HttpGet]
        public IActionResult GetCities()
        {
            var cityEntities = _cityInfoRepository.GetCities();
            var results = Mapper.Map<IEnumerable<CityWithoutPointsOfInterestDto>> (cityEntities);
            
            return Ok(results);
        }
        
        [HttpGet("{id}")]
        public IActionResult GetCity(int id, bool includePointsOfInterest = false)
        {
            var city = _cityInfoRepository.GetCity(id, includePointsOfInterest);
            if (city == null)
            {
                return NotFound();
            }
            if (includePointsOfInterest)
            {
                var cityResult = Mapper.Map<CityDto>(city);                
                return Ok(cityResult);
            }
            else
            {
                var cityResult = Mapper.Map<CityWithoutPointsOfInterestDto>(city);               
                return Ok(cityResult);
            }
        }
    }
}
