using Microsoft.EntityFrameworkCore;

namespace TelegramDownloadBot.Bot.Data
{
    public class DatabaseContext : DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {
        }
        public DbSet<CachedSearchResponse> CachedSearchResponses { get; set; }
        public DbSet<SearchResponse> SearchResponses { get; set; }
    }
}