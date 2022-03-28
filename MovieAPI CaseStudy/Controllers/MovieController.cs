using Case.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MovieAPI_CaseStudy.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MovieController : ControllerBase
    {
        public IMovieOperations _movieOperations;
        public MovieController(IMovieOperations movieOperations)
        {
            _movieOperations = movieOperations;
        }

        [Route("FetchMovies")]
        [HttpGet]
        public IActionResult FetchMovies()
        {
            _movieOperations.FetchMoviesFromAPI();
            return Ok();
        }

        [Route("GetMovies")]
        [HttpGet]
        public IActionResult GetMovies(int pageNumber)
        {
            try
            {
                var result = _movieOperations.GetMovies(pageNumber);
                return Ok(result.results);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("GetMovieById")]
        [HttpGet]
        public IActionResult GetMovieById(int movieId)
        {
            try
            {
                var result = _movieOperations.GetMovieById(movieId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("RateMovie")]
        [HttpPost]
        public async Task<IActionResult> RateMovie(int movieId, string comment, int rating)
        {
            try
            {
                await _movieOperations.RateMovie(movieId, comment, rating);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Route("SuggestMovie")]
        [HttpPost]
        public IActionResult SuggestMovie(string emailAddress)
        {
            _movieOperations.RecommendMovie(emailAddress);
            return Ok();
        }
    }
}
