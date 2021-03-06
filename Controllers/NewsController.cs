﻿using HackerNewsDemo.Models;
using HackerNewsDemo.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Linq;
using Microsoft.Extensions.Configuration;

namespace HackerNewsDemo.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class NewsController : Controller
    {
        private readonly INewsService _newsService;
        private IMemoryCache _cache;
        private readonly IConfiguration _configuration;
        private int _initialStoryFetchAmount;
        private int _initialCacheTimeSpanInSeconds;

        public NewsController(
            INewsService newsService,
            IMemoryCache memoryCache,
            IConfiguration configuration)
        {
            _newsService = newsService;
            _cache = memoryCache;
            _configuration = configuration;

            int.TryParse(_configuration.GetSection("HackerNewsInfo:initialStoryFetchAmount").Value, out _initialStoryFetchAmount);
            int.TryParse(_configuration.GetSection("HackerNewsInfo:initialCacheTimeSpanInSeconds").Value, out _initialCacheTimeSpanInSeconds);
        }

        [HttpGet]
        public IEnumerable<Story> Get(string textToSearchFor, int numberOfStoriesToReturn = 25)
        {
            textToSearchFor ??= string.Empty;
            List<Story> storiesForClient;
            var newestStory = _newsService.GetNewestStory().Result;

            // Does cache object of NewestStoryID exist?
            if (!_cache.TryGetValue(CacheKeys.NewestStoryID, out string cachedNewestStoryID))
            {
                _ = GoAndBuildCache(newestStory);
            }

            //  The latest story is still the most current.  Therefore, the ListOfCompleteStories hasn't changed
            if (newestStory == cachedNewestStoryID)
            {
                storiesForClient = ((List<Story>)_cache.Get(CacheKeys.ListOfCompleteStories))
                    .Where(x => x.title.Contains(textToSearchFor, StringComparison.OrdinalIgnoreCase))
                    .Take(numberOfStoriesToReturn)
                    .ToList();
            }
            else
            {
                storiesForClient = GoAndBuildCache(newestStory)
                    .Where(x => x.title.Contains(textToSearchFor, StringComparison.OrdinalIgnoreCase))
                    .Take(numberOfStoriesToReturn)
                    .ToList();
            }

            return storiesForClient;

        }

        private List<Story> GoAndBuildCache(string newestStoryID)
        {
            var storiesToReturn = new List<Story>();

            string cachedListOfStoryIDs = _newsService.GetAllStories().Result;

            List<int> listOfStoryIDs = JsonConvert.DeserializeObject<List<int>>(cachedListOfStoryIDs).Take(_initialStoryFetchAmount).ToList();

            BuildNewsStories(storiesToReturn, listOfStoryIDs);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromSeconds(_initialCacheTimeSpanInSeconds));

            // Save data in cache.
            _cache.Set(CacheKeys.NewestStoryID, newestStoryID, cacheEntryOptions);
            _cache.Set(CacheKeys.ListOfStoryIDs, listOfStoryIDs, cacheEntryOptions);
            _cache.Set(CacheKeys.ListOfCompleteStories, storiesToReturn, cacheEntryOptions);

            return storiesToReturn;
        }

        private void BuildNewsStories(List<Story> storiesToReturn, List<int> listOfStoryIDs)
        {
            foreach (int storyID in listOfStoryIDs)
            {
                var specificStory = _newsService.GetSingleStoryByID(storyID).Result;


                if (specificStory == null)
                    continue;

                Story storyMetaData = JsonConvert.DeserializeObject<Story>(specificStory);

                storiesToReturn.Add(storyMetaData);
            }
        }
    }
}


public static class CacheKeys
{
    public static string NewestStoryID { get { return "_NewestStoryID"; } }
    public static string ListOfStoryIDs { get { return "_ListOfStoryIDs"; } }
    public static string ListOfCompleteStories { get { return "_ListOfCompleteStories"; } }
}
