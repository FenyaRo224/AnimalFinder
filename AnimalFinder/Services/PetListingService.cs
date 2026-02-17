using AnimalFinder.Models;
using Supabase;
using Supabase.Postgrest;
using Supabase.Postgrest.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AnimalFinder.Services
{
    public class PetListingService
    {
        private readonly Supabase.Client _client;

        public PetListingService()
        {
            _client = SupabaseService.Client;
        }

        public async Task<bool> CreateAsync(PetListing listing)
        {
            try
            {
                if (string.IsNullOrEmpty(listing.Id))
                    listing.Id = Guid.NewGuid().ToString();

                listing.CreatedAt = DateTime.UtcNow;

                var response = await _client
                    .From<PetListing>()
                    .Insert(listing);

                return response.Models.Any();
            }
            catch
            {
                return false;
            }
        }

        public async Task<List<PetListing>> GetAllAsync()
        {
            try
            {
                var response = await _client
                    .From<PetListing>()
                    .Get();

                return response.Models;
            }
            catch
            {
                return new List<PetListing>();
            }
        }

        public async Task<List<PetListing>> GetByUserAsync(string userId)
        {
            try
            {
                var response = await _client
                    .From<PetListing>()
                    .Where(x => x.UserId == userId)
                    .Get();

                return response.Models;
            }
            catch
            {
                return new List<PetListing>();
            }
        }

        public async Task<bool> UpdateAsync(PetListing listing)
        {
            try
            {
                if (string.IsNullOrEmpty(listing.Id))
                    return false;

                // Только те поля, которые реально есть в модели!
                var query = _client
                    .From<PetListing>()
                    .Where(x => x.Id == listing.Id);

                // Обновляем только изменяемые поля (все, кроме Id и CreatedAt)
                query = query.Set(x => x.ListingType, listing.ListingType)
                             .Set(x => x.Species, listing.Species)
                             .Set(x => x.Breed, listing.Breed)
                             .Set(x => x.Color, listing.Color)
                             .Set(x => x.Age, listing.Age)
                             .Set(x => x.PhotoUrl, listing.PhotoUrl)
                             .Set(x => x.Location, listing.Location)
                             .Set(x => x.Description, listing.Description);

                var response = await query.Update();

                return response.Models.Any();
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteAsync(string id)
        {
            try
            {
                await _client
                    .From<PetListing>()
                    .Where(x => x.Id == id)
                    .Delete();

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}