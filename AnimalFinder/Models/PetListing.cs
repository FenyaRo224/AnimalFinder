using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AnimalFinder.Models
{
    [Table("pet_listings")]
    public class PetListing : BaseModel
    {
        [PrimaryKey("id")]
        public string Id { get; set; } = string.Empty;

        [Column("user_id")]
        public string UserId { get; set; } = string.Empty;

        [Column("listing_type")]
        public string ListingType { get; set; } = "lost"; // "lost" / "found"

        [Column("species")]
        public string Species { get; set; } = string.Empty;

        [Column("breed")]
        public string Breed { get; set; } = string.Empty;

        [Column("color")]
        public string Color { get; set; } = string.Empty;

        [Column("age")]
        public int? Age { get; set; }

        [Column("size")]
        public string? Size { get; set; } // "Маленький", "Средний", "Большой"

        [Column("gender")]
        public string? Gender { get; set; } // "Мальчик", "Девочка", "Неизвестно"

        [Column("photo_url")]
        public string? PhotoUrl { get; set; }

        [Column("location")]
        public string Location { get; set; } = string.Empty;

        [Column("description")]
        public string Description { get; set; } = string.Empty;

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("temperament")]
        public string Temperament { get; set; } = string.Empty;

        [Column("pet_name")]
        public string PetName { get; set; } = string.Empty;

        [Column("contact")]
        public string? Contact { get; set; }
    }
}