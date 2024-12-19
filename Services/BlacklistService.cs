using AonFreelancing.Contexts;
using AonFreelancing.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;
using System.Text;

namespace AonFreelancing.Services
{
    public class BlacklistService(MainAppContext mainAppContext) : MainDbService(mainAppContext)
    {
        public async Task AddTokenToBlacklist(string token, DateTime expiredAt,string email)
        {
            var tokenHash = HashToken(token);
            var blacklistEntry = new TokenBlacklist
            {
                Email = email,
                TokenHash = tokenHash,
                ExpiredAt = expiredAt,
                BlacklistedAt = DateTime.Now
            };
            await mainAppContext.TokensBlacklist.AddAsync(blacklistEntry);
        }
        public async Task DeleteTokenFromBlacklist(string storedEmail)
        {
            
            var storedToken = await mainAppContext.TokensBlacklist.FirstOrDefaultAsync(t=>t.Email== storedEmail);
             mainAppContext.TokensBlacklist.Remove(storedToken);
        }

        public async Task<bool> IsTokenBlacklisted(string token)
        {
            var tokenHash = HashToken(token);
            return await mainAppContext.TokensBlacklist.AnyAsync(b => b.TokenHash == tokenHash);
        }

        //public void RemoveExpiredTokens()
        //{
        //    var expiredTokens = mainAppContext.TokensBlacklist
        //        .Where(t => t.ExpiredAt < DateTime.Now);

        //    mainAppContext.TokensBlacklist.RemoveRange(expiredTokens);
        //}

        private string HashToken(string token)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(token));
                return Convert.ToBase64String(bytes);
            }
        }

    }
}
