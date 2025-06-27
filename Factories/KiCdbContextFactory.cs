using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using KiCData.Models;

namespace KiCData.Factories
{
    public class KiCdbContextFactory : IDesignTimeDbContextFactory<KiCdbContext>
    {
        public KiCdbContext CreateDbContext(string[] args)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .Build();

            var connectionString = config["Database:ConnectionString"];

            var optionsBuilder = new DbContextOptionsBuilder<KiCdbContext>();
            optionsBuilder.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));

            return new KiCdbContext(optionsBuilder.Options);
        }
    }
}