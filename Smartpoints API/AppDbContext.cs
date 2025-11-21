using Microsoft.EntityFrameworkCore;

namespace smartpoints_api;

public class AppDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseMySql(
            "server=localhost;database=smartpoints_rebirth;port=3306;user=root;password=admin",
            ServerVersion.Parse("8.0.30")
        );
    }
}