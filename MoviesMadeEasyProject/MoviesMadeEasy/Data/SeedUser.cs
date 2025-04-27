// Updated SeedData.cs (Create a copy in your Tests project)
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MoviesMadeEasy.Models;

namespace MoviesMadeEasy.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<IdentityUser>>();
            var dbContext = serviceProvider.GetRequiredService<UserDbContext>();

            await EnsureUserAsync(
                "testuser@example.com",
                "Ab+1234",
                true,
                userManager,
                dbContext
            );

            await EnsureUserAsync(
                "testuser2@example.com",
                "Ab+1234",
                false,
                userManager,
                dbContext
            );
        }

        private static async Task EnsureUserAsync(
            string email,
            string password,
            bool seedMovies,
            UserManager<IdentityUser> userManager,
            UserDbContext dbContext)
        {
            var aspUser = await userManager.FindByEmailAsync(email);
            if (aspUser == null)
            {
                aspUser = new IdentityUser
                {
                    UserName = email,
                    Email = email,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(aspUser, password);
                if (!result.Succeeded)
                {
                    Console.WriteLine($"Failed to create user {email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                    return;
                }
            }

            var customUser = await dbContext.Users.FirstOrDefaultAsync(u => u.AspNetUserId == aspUser.Id);
            if (customUser == null)
            {
                customUser = new User
                {
                    AspNetUserId = aspUser.Id,
                    FirstName = email.Split('@')[0],
                    LastName = "Test",
                    ColorMode = "Light",
                    FontSize = "Medium",
                    FontType = "Sans-serif"
                };
                dbContext.Users.Add(customUser);
                await dbContext.SaveChangesAsync();
            }

            if (seedMovies)
            {
                var defaultServices = new[] { "Hulu", "Disney+", "Netflix" };
                var services = await dbContext.StreamingServices
                    .Where(s => defaultServices.Contains(s.Name))
                    .ToListAsync();

                // Add streaming services if they don't exist
                if (!services.Any())
                {
                    dbContext.StreamingServices.AddRange(
                        new StreamingService { Name = "Hulu", Region = "US", BaseUrl = "https://auth.hulu.com/web/login", LogoUrl = "/images/hulu-Green-digital.png" },
                        new StreamingService { Name = "Disney+", Region = "US", BaseUrl = "https://www.disneyplus.com/login", LogoUrl = "/images/disney_logo_march_2024_050fef2e.png" },
                        new StreamingService { Name = "Netflix", Region = "US", BaseUrl = "https://www.netflix.com/login", LogoUrl = "/images/Netflix_Symbol_RGB.png" }
                    );
                    await dbContext.SaveChangesAsync();
                    services = await dbContext.StreamingServices.ToListAsync();
                }

                var existingServiceIds = await dbContext.UserStreamingServices
                    .Where(us => us.UserId == customUser.Id)
                    .Select(us => us.StreamingServiceId)
                    .ToListAsync();

                foreach (var svc in services)
                {
                    if (!existingServiceIds.Contains(svc.Id))
                    {
                        dbContext.UserStreamingServices.Add(new UserStreamingService
                        {
                            UserId = customUser.Id,
                            StreamingServiceId = svc.Id
                        });
                    }
                }
                await dbContext.SaveChangesAsync();

                var moviesToSeed = new List<Title>
                {
                    new Title
                    {
                        TitleName = "Her",
                        Year = 2013,
                        PosterUrl = "https://example.com/her.jpg",
                        Genres = "Romance,Drama",
                        Rating = "8.0",
                        Overview = "In a near future, a lonely writer develops an unlikely relationship with an operating system.",
                        StreamingServices = "Netflix",
                        LastUpdated = DateTime.UtcNow
                    },
                    new Title
                    {
                        TitleName = "Pokemon 4Ever",
                        Year = 2001,
                        PosterUrl = "https://example.com/pokemon4ever.jpg",
                        Genres = "Animation,Adventure",
                        Rating = "5.8",
                        Overview = "Ash and friends must save a Celebi from a hunter and a corrupted future.",
                        StreamingServices = "Hulu,Disney+",
                        LastUpdated = DateTime.UtcNow
                    }
                };

                foreach (var movie in moviesToSeed)
                {
                    bool exists = await dbContext.Titles
                        .AnyAsync(t => t.TitleName == movie.TitleName && t.Year == movie.Year);

                    if (!exists)
                    {
                        dbContext.Titles.Add(movie);
                    }
                }
                await dbContext.SaveChangesAsync();

                var herTitle = await dbContext.Titles.FirstOrDefaultAsync(t => t.TitleName == "Her" && t.Year == 2013);
                var pokemonTitle = await dbContext.Titles.FirstOrDefaultAsync(t => t.TitleName == "Pokemon 4Ever" && t.Year == 2001);

                var existingRecentlyViewed = await dbContext.RecentlyViewedTitles
                    .Where(rv => rv.UserId == customUser.Id)
                    .Select(rv => rv.TitleId)
                    .ToListAsync();

                if (herTitle != null && !existingRecentlyViewed.Contains(herTitle.Id))
                {
                    dbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
                    {
                        UserId = customUser.Id,
                        TitleId = herTitle.Id,
                        ViewedAt = DateTime.UtcNow.AddMinutes(-10)
                    });
                }

                if (pokemonTitle != null && !existingRecentlyViewed.Contains(pokemonTitle.Id))
                {
                    dbContext.RecentlyViewedTitles.Add(new RecentlyViewedTitle
                    {
                        UserId = customUser.Id,
                        TitleId = pokemonTitle.Id,
                        ViewedAt = DateTime.UtcNow.AddMinutes(-5)
                    });
                }

                await dbContext.SaveChangesAsync();
            }
        }
    }
}