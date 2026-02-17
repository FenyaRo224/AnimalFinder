using Supabase;
using System.Threading.Tasks;

public class SupabaseClientService
{
    private static SupabaseClientService _instance;
    public static SupabaseClientService Instance => _instance ??= new SupabaseClientService();

    private Client _client;

    private SupabaseClientService() { }

    public async Task Initialize()
    {
        var options = new SupabaseOptions
        {
            AutoConnectRealtime = false
        };

        _client = new Client(
            "https://wrblxjagrowujhcqmtxq.supabase.co",
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6IndyYmx4amFncm93dWpoY3FtdHhxIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NjM3MTQxNTYsImV4cCI6MjA3OTI5MDE1Nn0.yo-LN-ZN_vS3ZT3aO5gYzP2U6YqKHiy0Awub8668mXg",
            options);

        await _client.InitializeAsync();
    }

    public Supabase.Client Client => _client;
}
