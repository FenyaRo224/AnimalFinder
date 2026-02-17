using AnimalFinder.Models;
using Supabase;
using Supabase.Postgrest;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AnimalFinder.Services
{
    public class UserService
    {
        private readonly Supabase.Client _client = SupabaseService.Client;

        public async Task<User?> GetByIdAsync(string id)
        {
            try
            {
                var response = await _client
                    .From<User>()
                    .Where(x => x.Id == id)
                    .Single();

                return response;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetByIdAsync error: {ex}");
                return null;
            }
        }
        public async Task<bool> UpsertAsync(User user)
        {
            try
            {
                var response = await _client
                    .From<User>()
                    .Upsert(user, new QueryOptions { Upsert = true });

                bool success = response.Models != null && response.Models.Count > 0;
                System.Diagnostics.Debug.WriteLine($"UpsertAsync: success={success} for user {user.Id}");
                return success;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"UpsertAsync error: {ex}");
                return false;
            }
        }


        [Obsolete("Используй UpsertAsync вместо CreateAsync")]
        public async Task<bool> CreateAsync(User user) => await UpsertAsync(user);

        [Obsolete("Используй UpsertAsync вместо UpdateAsync")]
        public async Task<bool> UpdateAsync(User user) => await UpsertAsync(user);

        public async Task<List<User>> GetAllAsync()
        {
            try
            {
                var response = await _client
                    .From<User>()
                    .Get();

                return response.Models;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"GetAllAsync error: {ex}");
                return new List<User>();
            }
        }
    }
}