using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBddProject.Tests.Mocks
{
    public class MockMovieService : IMovieService
    {
        public Task<List<MoviesMadeEasy.Models.Movie>> SearchMoviesAsync(string query)
        {
            var movies = new List<MoviesMadeEasy.Models.Movie>();

            if (query?.Contains("Hunger Games", StringComparison.OrdinalIgnoreCase) == true)
            {
                movies.Add(new MoviesMadeEasy.Models.Movie
                {
                    Title = "The Hunger Games",
                    ReleaseYear = 2012,
                    ImageSet = new MoviesMadeEasy.Models.ImageSet
                    {
                        VerticalPoster = new MoviesMadeEasy.Models.VerticalPoster
                        {
                            W240 = "https://example.com/hunger-games.jpg"
                        }
                    },
                    Genres = new List<MoviesMadeEasy.Models.Genre>
                    {
                        new MoviesMadeEasy.Models.Genre { Name = "Action" },
                        new MoviesMadeEasy.Models.Genre { Name = "Adventure" },
                        new MoviesMadeEasy.Models.Genre { Name = "Sci-Fi" }
                    },
                    Rating = 72,
                    Overview = "Katniss Everdeen voluntarily takes her younger sister's place in the Hunger Games.",
                    StreamingOptions = new Dictionary<string, List<MoviesMadeEasy.Models.StreamingOption>>
                    {
                        {
                            "us", new List<MoviesMadeEasy.Models.StreamingOption>
                            {
                                CreateStreamingOption("Netflix"),
                                CreateStreamingOption("Apple TV"),
                                CreateStreamingOption("Prime Video")
                            }
                        }
                    }
                });
            }

            return Task.FromResult(movies);
        }

        private MoviesMadeEasy.Models.StreamingOption CreateStreamingOption(string serviceName)
        {
            return new MoviesMadeEasy.Models.StreamingOption
            {
                Service = new MoviesMadeEasy.Models.Service
                {
                    Name = serviceName,
                    Id = serviceName.ToLower().Replace(" ", "-")
                }
            };
        }
    }
}