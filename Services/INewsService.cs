using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HackerNewsDemo.Services
{
    public interface INewsService
    {
        Task<string> GetAllStories();
        Task<string> GetNewestStory();
        Task<string> GetSingleStoryByID(int id);
    }
}
