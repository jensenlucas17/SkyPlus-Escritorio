using Microsoft.EntityFrameworkCore;

namespace SkyPlus.Infrastructure.Data
{
    public class SkyPlusDbContext : DbContext
    {
        public SkyPlusDbContext(DbContextOptions<SkyPlusDbContext> options)
            : base(options)
        {
        }
    }
}