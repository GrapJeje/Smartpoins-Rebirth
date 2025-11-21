using smartpoints_api.apiRequests;

namespace smartpoints_api;

internal class Program
{
    private static ApiServer Server;
    
    public static void Main(string[] args)
    {
        Console.WriteLine("Connecting to database...");
        using var db = new AppDbContext();
        if (!db.Database.CanConnect()) 
            db.Database.EnsureCreated();
        Console.WriteLine("Connected to database");
        
        Server = new ApiServer("http://localhost:8080/");
    }
    
    public static ApiServer GetServer()
    {
        return Server;
    }
}