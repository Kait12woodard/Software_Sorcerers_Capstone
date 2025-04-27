using MoviesMadeEasy.DAL.Abstract;
using MoviesMadeEasy.Models;

namespace MyBddProject.Tests.Mocks;

public class MockMovieService : IMovieService
{
    public Task<List<Movie>> SearchMoviesAsync(string query)
    {
        var movies = new List<Movie>();

        // Create standardized response for tests
        if (query?.Contains("Hunger Games", StringComparison.OrdinalIgnoreCase) == true)
        {
            movies.Add(CreateHungerGamesMovie());
        }
        else if (!string.IsNullOrEmpty(query))
        {
            // Generic movie for any other query
            movies.Add(CreateGenericMovie(query));
        }

        return Task.FromResult(movies);
    }

    private Movie CreateHungerGamesMovie()
    {
        return new Movie
        {
            Title = "The Hunger Games",
            ReleaseYear = 2012,
            ImageSet = new ImageSet
            {
                VerticalPoster = new VerticalPoster
                {
                    W240 = "https://example.com/hunger-games.jpg"
                }
            },
            Genres = new List<Genre>
            {
                new Genre { Id = "1", Name = "Action" },
                new Genre { Id = "2", Name = "Adventure" },
                new Genre { Id = "3", Name = "Sci-Fi" }
            },
            Rating = 72,
            Overview = "Katniss Everdeen volunteers as tribute to participate in a fight to the death.",
            StreamingOptions = new Dictionary<string, List<StreamingOption>>
            {
                {
                    "us", new List<StreamingOption>
                    {
                        CreateStreamingOption("Netflix"),
                        CreateStreamingOption("Apple TV"),
                        CreateStreamingOption("Prime Video")
                    }
                }
            }
        };
    }

    private Movie CreateGenericMovie(string title)
    {
        return new Movie
        {
            Title = title,
            ReleaseYear = 2023,
            ImageSet = new ImageSet
            {
                VerticalPoster = new VerticalPoster
                {
                    W240 = "https://example.com/generic-movie.jpg"
                }
            },
            Genres = new List<Genre>
            {
                new Genre { Id = "1", Name = "Action" }
            },
            Rating = 65,
            Overview = $"A movie about {title}.",
            StreamingOptions = new Dictionary<string, List<StreamingOption>>
            {
                {
                    "us", new List<StreamingOption>
                    {
                        CreateStreamingOption("Netflix")
                    }
                }
            }
        };
    }

    private StreamingOption CreateStreamingOption(string serviceName)
    {
        return new StreamingOption
        {
            Service = new Service
            {
                Name = serviceName,
                Id = serviceName.ToLower().Replace(" ", "-")
            },
            Link = $"https://{serviceName.ToLower().Replace(" ", "")}.com"
        };
    }
}