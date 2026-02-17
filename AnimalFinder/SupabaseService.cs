using Supabase;

namespace AnimalFinder.Services
{
    public static class SupabaseService
    {
        public static Supabase.Client Client { get; private set; }

        public static async Task InitializeAsync()
        {
            Client = new Supabase.Client(
                "https://wrblxjagrowujhcqmtxq.supabase.co",
                "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6IndyYmx4amFncm93dWpoY3FtdHhxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM3MTQxNTYsImV4cCI6MjA3OTI5MDE1Nn0.yo-LN-ZN_vS3ZT3aO5gYzP2U6YqKHiy0Awub8668mXg",
                new SupabaseOptions
                {
                    AutoConnectRealtime = false,
                    AutoRefreshToken = true
                }
            );

            await Client.InitializeAsync();
        }
    }
}
