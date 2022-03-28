using Case.Data.Model.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case.Core.Interfaces
{
    public interface IMovieOperations
    {
        Task FetchMoviesFromAPI();
        MovieListResultModel GetMovies(int pageNumber);
        Movie GetMovieById(int id);
        Task RateMovie(int movieId, string comment, int rating);
        Task RecommendMovie(string email);
    }
}
