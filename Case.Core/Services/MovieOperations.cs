using Case.Core.Interfaces;
using Case.Data.Model.Notification;
using Case.Data.Model.Redis;
using Case.WebApiCaller;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case.Core.Services
{
    public class MovieOperations : IMovieOperations
    {
        private readonly IDistributedCache _distributedCache;
        private static IConfiguration _config;
        private INotificationService _notificationService;
        public MovieOperations(IDistributedCache distributedCache, IConfiguration config, INotificationService notificationService)
        {
            _distributedCache = distributedCache;
            _config = config;
            _notificationService = notificationService;
        }
        public async Task FetchMoviesFromAPI()
        {
            var url = $"{_config.GetSection("TheMovieDatabaseAPI:apiDomain").Value}{_config.GetSection("TheMovieDatabaseAPI:discoverMovies").Value}";

            List<MovieListResultModel> moviesByPages = new List<MovieListResultModel>();

            //await _distributedCache.RemoveAsync("movieList");

            for (int i = 1; i <= 10; i++) //Fetching only the first 10 pages
            {
                var urlWithParameters = string.Format("?api_key={0}&page={1}", _config.GetSection("TheMovieDatabaseAPI:apiKey").Value, i);

                //Dictionary<string, string> pairs = new Dictionary<string, string>
                //{
                //    { "api_key", _config.GetSection("TheMovieDatabaseAPI:apiKey").Value },
                //    { "page", i.ToString() },
                //};

                MovieListResultModel result = ApiCaller.RunAsync<MovieListResultModel>(url, urlWithParameters).GetAwaiter().GetResult();

                moviesByPages.Add(result);
            }

            await _distributedCache.SetStringAsync("movieList", JsonConvert.SerializeObject(moviesByPages));
        }

        public Movie GetMovieById(int id)
        {
            var model = this.GetRedisValue<List<MovieListResultModel>>("movieList").Result;

            if (model == null)
            {
                throw new Exception("No movie found in redis store. Try calling FetchMovies from TheMovieDatabase.");
            }
            else
            {
                var result = model.SelectMany(s => s.results).FirstOrDefault(x => x.id == id);

                return result;
            }
        }

        public MovieListResultModel GetMovies(int pageNumber)
        {
            var model = this.GetRedisValue<List<MovieListResultModel>>("movieList").Result;

            if (model == null)
            {
                throw new Exception("No movie found in redis store. Try calling FetchMovies from TheMovieDatabase.");
            }
            else
            {
                var result = model.Where(x => x.page == pageNumber)?.FirstOrDefault();

                return result;
            }
        }

        public async Task RateMovie(int movieId, string comment, int rating)
        {
            var model = this.GetRedisValue<List<MovieListResultModel>>("movieList").Result;

            if (model == null)
            {
                throw new Exception("No movie found in redis store. Try calling FetchMovies from TheMovieDatabase.");
            }
            else if (rating < 0 || rating > 10)
            {
                throw new Exception("Rating value cannot be lower than 0 or higher than 10");
            }
            else
            {
                Movie movie = model.SelectMany(s => s.results).FirstOrDefault(x => x.id == movieId);

                if (movie != null)
                {
                    movie.userReview = comment;
                    movie.userVote = rating;
                }

                await _distributedCache.SetStringAsync("movieList", JsonConvert.SerializeObject(model));
            }
        }

        public Task RecommendMovie(string emailAddress)
        {
            var url = $"{_config.GetSection("TheMovieDatabaseAPI:apiDomain").Value}{_config.GetSection("TheMovieDatabaseAPI:popularMovies").Value}";
            var urlWithParameters = string.Format("?api_key={0}", _config.GetSection("TheMovieDatabaseAPI:apiKey").Value);

            MovieListResultModel popularMovies = ApiCaller.RunAsync<MovieListResultModel>(url, urlWithParameters).GetAwaiter().GetResult();

            int movieCount = popularMovies.results.Count;

            Random r = new Random();
            int randomIndex = r.Next(0, movieCount - 1);

            Movie randomlyPickedPopularMovie = popularMovies.results[randomIndex];

            MailRequest mail = new MailRequest
            {
                Subject = "Recommended Movie",
                To = emailAddress,
                Message = $"Here's the movie that we recommend you to watch: {JsonConvert.SerializeObject(randomlyPickedPopularMovie)}"
            };

            var result = _notificationService.SendNotificationAsync(mail);

            return result;
        }

        private async Task<T> GetRedisValue<T>(string key)
        {
            string redisValue = await _distributedCache.GetStringAsync(key);

            return string.IsNullOrEmpty(redisValue) ? default(T) : JsonConvert.DeserializeObject<T>(redisValue);
        }
    }
}
