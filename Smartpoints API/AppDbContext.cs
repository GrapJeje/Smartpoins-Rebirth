using Microsoft.EntityFrameworkCore;
using Smartpoints_Api.Models;
using dotenv.net;

namespace smartpoints_api
{
    public class AppDbContext : DbContext
    {
        public DbSet<Point> Points { get; set; } = null!;
        public DbSet<User> Users { get; set; }
        public DbSet<Subjects> Subjects { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            var envPath = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".env");
            DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { envPath }));

            var server = Environment.GetEnvironmentVariable("DB_SERVER") ?? "localhost";
            var database = Environment.GetEnvironmentVariable("DB_DATABASE") ?? "smartpoints_rebirth";
            var port = Environment.GetEnvironmentVariable("DB_PORT") ?? "3306";
            var user = Environment.GetEnvironmentVariable("DB_USER") ?? "root";
            var password = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? "";

            var connectionString = $"server={server};database={database};port={port};user={user};password={password}";
            optionsBuilder.UseMySql(connectionString, ServerVersion.Parse("8.0.30"));
        }

    }
}