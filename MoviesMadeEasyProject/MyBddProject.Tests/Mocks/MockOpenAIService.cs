using System.Collections.Generic;
using System.Threading.Tasks;
using MoviesMadeEasy.DAL.Abstract;

namespace MyBddProject.Tests.Mocks
{
    public class MockOpenAIService : IOpenAIService
    {
        public Task<List<MovieRecommendation>> GetSimilarMoviesAsync(string title)
        {
            var recommendations = new List<MovieRecommendation>
            {
                new MovieRecommendation { Title = "The Maze Runner", Year = 2014, Reason = "Similar dystopian theme" },
                new MovieRecommendation { Title = "Divergent", Year = 2014, Reason = "Features a strong female protagonist in a dystopian future" },
                new MovieRecommendation { Title = "Battle Royale", Year = 2000, Reason = "Similar survival competition premise" },
                new MovieRecommendation { Title = "The Giver", Year = 2014, Reason = "Dystopian society with controlled roles" },
                new MovieRecommendation { Title = "Ender's Game", Year = 2013, Reason = "Young protagonists trained for combat" }
            };

            return Task.FromResult(recommendations);
        }

        public Task<string> GetChatCompletionAsync(string prompt)
        {
            return Task.FromResult("This is a mock response from the AI.");
        }
    }
}