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

        public async Task<bool> Save(long raterUserId, long ratedUserId, double ratingValue, string comment)
        {
            var rating = new Rating
            {
                RaterUserId = raterUserId,
                RatedUserId = ratedUserId,
                RatingValue = ratingValue,
                Comment = comment,
                CreatedAt = DateTime.Now
            };

            _mainAppContext.Ratings.Add(rating);
            await _mainAppContext.SaveChangesAsync();

            return true;
        }


        public async Task<List<Rating>> GetRatingsForUserAsync(long userId)
        {
            return await _mainAppContext.Ratings
                .Where(r => r.RatedUserId == userId)
                .ToListAsync();
        }


        public async Task<double> GetAverageRatingForUserAsync(long userId)
        {
            var ratings = await _mainAppContext.Ratings
                .Where(r => r.RatedUserId == userId)
                .ToListAsync();

            //if (ratings.Count == 0)
            //{
            //    return 0; 
            //}

            return ratings.Average(r => r.RatingValue);
        }

        // Check if the user has rating or not
        public async Task<bool> IsRatingAlreadyGivenAsync(long raterUserId, long ratedUserId)
        {
            return await _mainAppContext.Ratings
                .AnyAsync(r => r.RaterUserId == raterUserId && r.RatedUserId == ratedUserId);
        }

    }
}