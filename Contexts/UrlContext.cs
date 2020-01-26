using LinkShortenService.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkShortenService.Contexts
{
    public class UrlContext : DbContext
    {
        public DbSet<UrlModel> URLs { get; set; }
        public UrlContext(DbContextOptions<UrlContext> options)
            : base(options)
        {
            Database.EnsureCreated();
        }
    }
}