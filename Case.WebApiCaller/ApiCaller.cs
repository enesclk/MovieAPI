using Newtonsoft.Json;
using RestSharp;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace Case.WebApiCaller
{
    public static class ApiCaller
    {
        private static HttpClient GetHttpClient(string url)
        {
            var client = new HttpClient { BaseAddress = new Uri(url) };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }

        private static async Task<T> GetAsync<T>(string url, string urlParameters)
        {
            try
            {
                using (var client = GetHttpClient(url))
                {
                    HttpResponseMessage response = client.GetAsync(urlParameters).Result; //await
                    if (response.IsSuccessStatusCode)
                    {
                        var json = response.Content.ReadAsStringAsync().Result; //await
                        var result = JsonConvert.DeserializeObject<T>(json);
                        return result;
                    }

                    return default(T);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return default(T);
            }
        }

        public static async Task<T> RunAsync<T>(string url, string urlParameters)
        {
            return await GetAsync<T>(url, urlParameters);
        }
    }
}
