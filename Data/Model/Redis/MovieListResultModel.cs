using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Case.Data.Model.Redis
{
    public class MovieListResultModel
    {
        public int page { get; set; }
        [JsonIgnore]
        public List<Movie> results { get; set; }
        //public int total_pages { get; set; }
        //public int total_results { get; set; }

    }
}
