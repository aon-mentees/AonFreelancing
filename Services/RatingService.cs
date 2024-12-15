using AonFreelancing.Models;
using AonFreelancing.Models.Requests;
using AonFreelancing.Contexts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;
using AonFreelancing.Models.DTOs;

namespace AonFreelancing.Services
{
    public class RatingService
    {
        private readonly MainAppContext _mainAppContext;

        public RatingService(MainAppContext mainAppContext)
        {
            _mainAppContext = mainAppContext;
        }

        public async Task Save(Rating rating)
        {
            await _mainAppContext.Ratings.AddAsync(rating);
            await _mainAppContext.SaveChangesAsync();
        }


        public async Task<List<Rating>> GetRatingsForUserAsync(long userId, int page, int pageSize)
        {
            return await _mainAppContext.Ratings
                .Where(r => r.RatedUserId == userId)
                .OrderByDescending(r => r.CreatedAt)
                .Skip(page * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }


        public async Task<double> GetAverageRatingForUserAsync(long userId)
        {
            var ratings = await _mainAppContext.Ratings
                .Where(r => r.RatedUserId == userId)
                .ToListAsync();

            if (ratings.Count == 0)
                return double.NaN;

            //return ratings.DefaultIfEmpty(double.NaN).Average(r => r.RatingValue); Just for me :) don't use it. 
            return ratings.Average(r => r.RatingValue);
        }

        // Check if the user has rating or not
        public async Task<bool> IsRatingAlreadyGivenAsync(long raterUserId, long ratedUserId)
        {
            return await _mainAppContext.Ratings
                .AnyAsync(r => r.RaterUserId == raterUserId && r.RatedUserId == ratedUserId);
        }

        public async Task<UserRatingDTO> GetRatingCalculationForUserAsync(long userId)
        {
            var ratings = await _mainAppContext.Ratings
                .Where(r => r.RatedUserId == userId)
                .ToListAsync();

            if (ratings != null && ratings.Any())
            {
                double avgRating = ratings.Average(r => r.RatingValue);
                int totalRating = ratings.Count;

                int highCount = ratings.Count(r => r.RatingValue >= 8);                      // Ratings 8-10 are high
                int midCount = ratings.Count(r => r.RatingValue >= 4 && r.RatingValue < 8); // Ratings 4-7.9 are mid
                int lowCount = ratings.Count(r => r.RatingValue < 4);                       // Ratings 1-3.9 are low

                string highPercentage = $"{(double)highCount / totalRating * 100:0.##}%";
                string midPercentage = $"{(double)midCount / totalRating * 100:0.##}%";
                string lowPercentage = $"{(double)lowCount / totalRating * 100:0.##}%";

                return new UserRatingDTO(avgRating, highPercentage, midPercentage, lowPercentage, totalRating);
            }

            return new UserRatingDTO(double.NaN, "0%", "0%", "0%", 0);
        }
    }
}