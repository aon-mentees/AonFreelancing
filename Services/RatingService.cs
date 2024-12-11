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

    }
}