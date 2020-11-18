using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Threading.Tasks;


namespace HackerNewsDemo.Services
{
    public class NewsService : INewsService
    {
        private HttpClient _client = new HttpClient();
        private readonly IConfiguration Configuration;
        private readonly string _prependUrl;
        private readonly string _newstoriesUrl;
        private readonly string _maxitemUrl;
        private readonly string _singelStoryUrl;



        public NewsService(IConfiguration configuration)
        {
            Configuration = configuration;
            _prependUrl = Configuration.GetSection("HackerNewsInfo:prependUrl").Value;
            _newstoriesUrl = Configuration.GetSection("HackerNewsInfo:newstoriesUrl").Value;
            _maxitemUrl = Configuration.GetSection("HackerNewsInfo:maxitemUrl").Value;
            _singelStoryUrl = Configuration.GetSection("HackerNewsInfo:singelStoryUrl").Value;
            _client.BaseAddress = new Uri(_prependUrl);
        }

        public Task<string> GetNewestStory()
        {
            return _client.GetAsync($"{_maxitemUrl}").Result.Content.ReadAsStringAsync();
        }

        public Task<string> GetAllStories()
        {
            return _client.GetAsync($"{_newstoriesUrl}").Result.Content.ReadAsStringAsync();
        }

        public Task<string> GetSingleStoryByID(int id)
        {
            return _client.GetAsync($"{_singelStoryUrl}{id}.json").Result.Content.ReadAsStringAsync();

        }
    }
}
