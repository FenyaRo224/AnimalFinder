using System;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;

namespace AnimalFinder.Models
{
    [Table("profiles")]
    public class User : BaseModel
    {
        [PrimaryKey("id", false)]  
        public string Id { get; set; } = string.Empty;

        [Column("email")]
        public string? Email { get; set; }

        [Column("password_hash")]
        public string? PasswordHash { get; set; }

        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Column("phone")]
        public string? Phone { get; set; }

        [Column("avatar_url")]
        public string? AvatarUrl { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }
    }
}
    